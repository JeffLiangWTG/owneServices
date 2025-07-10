using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.Customs.CA.Business
{
	//TODO: Remove this class if not used by web
	public class ReleaseStatusDocumentWrapper : NonPersistentBusinessObject, IObsoleteValidation, IReleaseStatusWrapper
	{
		public ReleaseStatusDocumentWrapper()
			: this(new ReleaseStatus(null))
		{
		}

		public ReleaseStatusDocumentWrapper(ReleaseStatus releaseStatus)
			: base(releaseStatus.Factory)
		{
			this.releaseStatus = releaseStatus;
		}

		public ReleaseStatusDocumentWrapper(Enterprise.Messaging.Business.EDIMessage message)
			: base(message.Factory)
		{
			Argument.NotNull(message, "message");
			releaseStatus = new ReleaseStatus(message as EDIReleaseMessage);
		}

		public ZString ServiceOptionDescription
		{
			get
			{
				var serviceOption = releaseStatus.RL_ServiceOption;
				return (serviceOption + " " + ServiceOptions.GetShortDescription(serviceOption)).Trim();
			}
		}

		public ZString TransactionNumber
		{
			get
			{
				var result = releaseStatus.RL_TransactionNumber;
				return result.Length == 14 ? ZString.Format("{0} {1} {2}", result.Substring(0, 5), result.Substring(5, 8), result.Substring(13, 1)) : result;
			}
		}

		public ZString ProcessingIndicatorDescription
		{
			get { return releaseStatus.ProcessingIndicatorCodeDescription; }
		}

		internal string ReleaseStatus
		{
			get { return releaseStatus.RL_ReleaseStatus; }
		}

		public ZDateTime ProcessingDate
		{
			get { return releaseStatus.RL_ProcessingDate; }
		}

		public ZDateTime ReleaseDate
		{
			get { return releaseStatus.RL_ReleaseDate; }
		}

		public ZString CCN
		{
			get { return releaseStatus.RL_CargoControlNumber; }
		}

		public ZString DeliveryInstructions1
		{
			get { return DeliveryInstructions.FirstOrDefault(); }
		}

		public ZString DeliveryInstructions2
		{
			get { return DeliveryInstructions.ElementAtOrDefault(2); }
		}

		IEnumerable<ZString> DeliveryInstructions
		{
			get { return releaseStatus.RL_DeliveryInstructions.Split('\r', '\n'); }
		}

		public ZString ReleaseOffice
		{
			get
			{
				var result = releaseStatus.RL_ReleaseOffice.IsEmpty ? ZString.Empty : releaseStatus.RL_ReleaseOffice.PadLeft(4, '0');
				if (!result.IsEmpty)
				{
					var cbsaOffice = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, result, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
					if (cbsaOffice != null)
					{
						result += " - " + cbsaOffice.ZZD_Description;
					}
				}
				return result;
			}
		}

		public ZString Warehouse
		{
			get
			{
				var result = releaseStatus.RL_WarehouseCode.TrimStart('0');
				if (!result.IsEmpty)
				{
					var warehouse = CACSubLocation.Load(Factory, result);
					if (warehouse != null)
					{
						result += " - " + warehouse.Description;
					}
				}
				return result;
			}
		}

		public ZString Containers
		{
			get { return new ZStringBuilder(releaseStatus.Containers).ToStringWithDelimiterBetweenAppends(", "); }
		}

		readonly ReleaseStatus releaseStatus;
	}
}
