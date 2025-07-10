
namespace Enterprise.DocumentWrappers.Customs
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.DocumentWrappers;
	using static Enterprise.Integration.Customs.CA;

	[AllowPublicConstructor]
	public class DocReleaseStatus : DocBaseWrapper
	{
		public DocReleaseStatus(object releaseStatus, BusinessObjectFactory factory)
			: base(releaseStatus, factory)
		{
		}

		public static DocReleaseStatus New(IReleaseStatus releaseStatus, BusinessObjectFactory factoryToWrap)
		{
			return releaseStatus == null ? null : new DocReleaseStatus(releaseStatus, factoryToWrap);
		}

		public ZString ServiceOptionDescription
		{
			get
			{
				return ReleaseStatus.GetServiceOptionDescription();
			}
		}

		public ZString TransactionNumberFormatted
		{
			get
			{
				var result = ReleaseStatus.RL_TransactionNumber;
				return result.Length == 14 ? ZString.Format("{0} {1} {2}", result.Substring(0, 5), result.Substring(5, 8), result.Substring(13, 1)) : result;
			}
		}

		public ZString ReleaseStatusDescription
		{
			get { return ReleaseStatus.ProcessingIndicatorCodeDescription; }
		}

		public ZDateTime ProcessingDate
		{
			get { return ReleaseStatus.RL_ProcessingDate; }
		}

		public ZDateTime ReleaseDate
		{
			get { return ReleaseStatus.RL_ReleaseDate; }
		}

		public ZString CargoControlNumber
		{
			get { return ReleaseStatus.RL_CargoControlNumber; }
		}

		public ZString DeliveryInstructions
		{
			get { return ReleaseStatus.RL_DeliveryInstructions; }
		}

		public ZString ReleaseOfficeCodeDescription
		{
			get
			{
				var result = ReleaseStatus.RL_ReleaseOffice.IsEmpty ? ZString.Empty : ReleaseStatus.RL_ReleaseOffice.PadLeft(4, '0');
				if (!result.IsEmpty)
				{
					var cbsaOffice = Enterprise.Customs.Universal.ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, result, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
					if (cbsaOffice != null)
					{
						result += " - " + cbsaOffice.ZZD_Description;
					}
				}
				return result;
			}
		}

		public ZString WarehouseCodeDescription
		{
			get
			{
				return ReleaseStatus.GetWarehouseCodeDescription();
			}
		}

		public ZString Containers
		{
			get { return new ZStringBuilder(ReleaseStatus.Containers).ToStringWithDelimiterBetweenAppends(", "); }
		}

		IReleaseStatus ReleaseStatus
		{
			get { return (IReleaseStatus)WrappedObject; }
		}
	}
}
