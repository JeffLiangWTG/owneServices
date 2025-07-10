using System.Linq;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class ConsolidatedDeclarationValidation : Customs.Business.ConsolidatedDeclarationValidation
	{
		public ConsolidatedDeclarationValidation(ConsolidatedDeclaration parent) : base(parent)
		{
		}

		new ConsolidatedDeclaration Parent => (ConsolidatedDeclaration)base.Parent;

		protected override void CheckCongruence()
		{
			if (!Parent.JobDeclarations.IsCongruentOn(dec => dec.JE_TransportMode, dec => dec.JE_OH_Importer, dec => dec.JE_MessageSubType, dec => dec.JE_MasterBill, dec => dec.JE_DateOfArrival.Date))
			{
				Parent.AddRowError(Res.GetString("45D0F17B-100E-40DC-9399-6BBBB631EEC7", "Consolidated declarations must be of the same Importer, Transport Mode, Dec Type, Master Bill and Discharge ETA."));
			}
		}

		protected override void CheckNumberOfEntryLines()
		{
			var totalEntryLines = Parent.JobDeclarations.Sum(x => x.ActiveEntryHeaders.Cast<CusEntryHeader>().Sum(y => y.MergedLines.Count));
			var limit = AUCustomsDataRegistry.Instance.MaxNumberOfEntryLinesAcceptedAtCustoms.Value;
			if (totalEntryLines > limit)
			{
				var message = Res.GetString("ea05caf0-eb3a-4821-9921-6cc024385a46", "The total number of entry lines ({0}) has exceeded the maximum number Customs accepts and this consolidated entry will fail. Currently the maximum number Customs accepts is {1}.", totalEntryLines, limit);
				Parent.AddRowError(message);
			}
		}
	}
}
