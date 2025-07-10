using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IOrgHeaderWrapper
	{
		ZString GoodsOwnerPartyID { get; }
		bool IsCompanyOrg { get; }
	}

	public class OrgHeaderWrapper : NonPersistentBusinessObject, IOrgHeaderWrapper, IObsoleteValidation
	{
		public OrgHeaderWrapper(OrgHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}

		public CLREGInfoProvider CLREGInfoProvider
		{
			get
			{
				if (clREGInfoProvider == null)
				{
					var query = new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.AUREG);
					query.AddToFilter(CusAddInfoSchema.B7_ParentID, header.PK);

					clREGInfoProvider = Factory.LoadTop1<CLREGInfoProvider>(query);

					if (clREGInfoProvider == null)
					{
						clREGInfoProvider = Factory.New<CLREGInfoProvider>();
						clREGInfoProvider.B7_ParentID = header.PK;
						clREGInfoProvider.B7_ParentTableCode = header.TablePrefix;
					}
					clREGInfoProvider.SetUpDefaultValuesIfNeeded(this);
				}
				return clREGInfoProvider;
			}
		}
		CLREGInfoProvider clREGInfoProvider;

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(header);
					fMessages.Load();
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		public OrgHeader OrgHeader
		{
			get { return header; }
		}
		protected readonly OrgHeader header;

		public ZBool HasCCIDWithSameAddress
		{
			get
			{
				var result = false;
				var address1 = CLREGInfoProvider.ZA_Bsn1;
				var address2 = CLREGInfoProvider.ZA_Bsn2;
				var city = CLREGInfoProvider.ZA_BsnCity;
				var postcode = CLREGInfoProvider.ZA_BsnPostCode;
				var configCodes = header.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
				if (address1.IsEmpty && address2.IsEmpty && city.IsEmpty && postcode.IsEmpty)
				{
					result = configCodes.Any(x => x.PremisesAddress == null);
				}
				else
				{
					result = configCodes.Any(x =>
					{
						var premisesAddress = x.PremisesAddress;
						return premisesAddress != null && premisesAddress.Address1 == address1 && premisesAddress.Address2 == address2
							&& premisesAddress.City == city && premisesAddress.Postcode == postcode;
					});
				}

				return result;
			}
		}

		#region IOrgHeaderWrapper Members

		public ZString GoodsOwnerPartyID
		{
			get { return !header.LocalBusinessRegNo.IsEmpty ? header.LocalBusinessRegNo : header.GetCustomsClientID(); }
		}

		bool IOrgHeaderWrapper.IsCompanyOrg
		{
			get { return header.PK == GlbCompany.CurrentCompany.GC_OH_OrgProxy; }
		}

		#endregion
	}
}
