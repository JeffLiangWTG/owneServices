using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public interface IReconDeclaration
			{
				ZGuid PK { get; }
				ZString ImporterContactEmail { get; }
				ZString ImporterContactName { get; }
				ZString ImporterContactPhone { get; }
				ZString ImporterName { get; }
				ZString ReconEntryNumberFormatted { get; }
				ZString US_Comment { get; }
				ZDateTime US_EstimatedEntryDate { get; }
				ZString US_IssueCode { get; }
				ZString US_SchDEntry { get; }
				ZString US_TeamNo { get; }
				ZString FilingPortDescription { get; }
				ZString TeamNoDescription { get; }
				ZString US_IssueCodeDescription { get; }
				IStaff CusAgent { get; }
				IUSOrgHeader ImporterOfRecord { get; }
				IUSIORWrapper IORWrapper { get; }
			}

			public interface IUSOrgHeader
			{
				ZString OH_FullName { get; }
			}

			public interface IUSIORWrapper
			{
				IUSOrgAddress MainAddress { get; }
			}

			public interface IUSOrgAddress
			{
				ZString AddressAsASingleLineWithoutCompanyName { get; }
			}
		}
	}
}
