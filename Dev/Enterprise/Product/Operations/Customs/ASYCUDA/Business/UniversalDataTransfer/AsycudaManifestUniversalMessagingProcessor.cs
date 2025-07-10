using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaManifestUniversalMessagingProcessor : Integration.Customs.ASYCUDA.IAsycudaManifestUniversalMessagingProcessor
	{
		public AsycudaManifestUniversalMessagingProcessor(BusinessObject rootObject, ZString recipientCode)
		{
			this.rootObject = rootObject;
			this.recipientCode = recipientCode;
		}

		readonly BusinessObject rootObject;
		readonly ZString recipientCode;

		public void Process(INotifications notifications, CancellationToken unused = new CancellationToken())
		{
			var countryCode = GetCountryCodeFromRecipientCode();
			var header = GetManifestFromBusinessObject(rootObject);

			if (countryCode.IsEmpty)
			{
				notifications.AddError(Invariant($"Unsupported Trigger Action Recipient: '{recipientCode}'"));
			}
			else if (header == null)
			{
				notifications.AddError(Invariant($"Could not obtain manifest from root object: '{rootObject.TableName}'"));
			}
			else if (header.AMA_RN_NKCountry != countryCode)
			{
				notifications.AddError(Invariant($"Country '{countryCode}' is not configured for manifest"));
			}
			else
			{
				if (header.IsBillLevelManifestType)
				{
					var bills = header.Bills.OfType<AsycudaBill>().Where(bill => !bill.HasManifestBeenSubmittedToCustoms).ToArray();
					if (bills.Any())
					{
						var context = header.GetCurrentManifestContext();
						context.ManifestSendBills = bills;

						var messageParents = bills.ToList<IMessageParent>();
						header.ApplicationBusinessProvider.GetUniversalMessagingHelper(notifications).SendViaEHub(header, header.AMA_ManifestType, MessageSubTypeCodes.Codes.Original, messageParents);
					}
					else
					{
						notifications.AddError("No suitable bills to send manifest for were found");
					}
				}
				else
				{
					if (!header.HasManifestBeenSubmittedToCustoms)
					{
						header.ApplicationBusinessProvider.GetUniversalMessagingHelper(notifications).SendViaEHub(header, header.AMA_ManifestType, MessageSubTypeCodes.Codes.Original, new IMessageParent[] { header });
					}
				}
			}
		}

		ZString GetCountryCodeFromRecipientCode()
		{
			switch (recipientCode)
			{
				case MessageRecipientPartyTypeList.Codes.USAirAMS:
					return Core.Constants.CountryCodes.UnitedStates;
				default:
					return ZString.Empty;
			}
		}

		internal static AsycudaManifestHeader GetManifestFromBusinessObject(BusinessObject businessObject) => businessObject as AsycudaManifestHeader ?? GetManifestFromParentBusinessObject(businessObject);

		static AsycudaManifestHeader GetManifestFromParentBusinessObject(BusinessObject parent)
		{
			var query = new ZQuery(AsycudaManifestHeaderSchema.AMA_ParentId, parent.PK);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ParentTableCode, parent.TablePrefix);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, SQLComparisonOperator.NotEqual, AsycudaManifestHeader.ApplicationCode_ZAOutturnGateInOut);
			query.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			return parent.Factory.Load<AsycudaManifestHeader>(query).OrderBy(x => x.AMA_SystemCreateTimeUtc).FirstOrDefault();
		}
	}
}
