using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgOpportunityValueLookups : OrgOpportunityValueLookups
	{
		public EDIOrgOpportunityValueLookups(EDIOrgOpportunityValue value) : base(value)
		{
		}

		#region Value Types List

		protected override CodeDescriptionPairList GetActiveValueTypesList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair(ValueTypeConstants.Sales, "Licence Sale Revenue");
			list.AddPair(ValueTypeConstants.Maintenance, "Licence Maintenance Revenue");
			list.AddPair(ValueTypeConstants.OnDemand, "Licence On Demand Revenue");
			list.AddPair(ValueTypeConstants.ProfServices, "Professional Services Revenue");
			list.AddPair(ValueTypeConstants.SER, "eServices Revenue");
			list.AddPair(ValueTypeConstants.Training, "Training Revenue");
			list.AddPair(ValueTypeConstants.PerTransaction, "Per Transaction Revenue (ie ediWebTracker)");
			list.AddPair(ValueTypeConstants.Other, "Other Revenue");

			return list;
		}

		protected override CodeDescriptionPairList GetValueTypesList()
		{
			CodeDescriptionPairList list = GetActiveValueTypesList();
			list.AddPair(ValueTypeConstants.Rental, "Licence Rental Revenue");
			return list;
		}

		public static class ValueTypeConstants
		{
			public const string Sales = "SAL";
			public const string Maintenance = "MAI";
			public const string Rental = "REN";
			public const string ProfServices = "PSR";
			public const string Training = "TRN";
			public const string PerTransaction = "TRA";
			public const string OnDemand = "ODM";
			public const string Other = "OTH";
			public const string SER = "SER";
		}

		#endregion
	}
}

