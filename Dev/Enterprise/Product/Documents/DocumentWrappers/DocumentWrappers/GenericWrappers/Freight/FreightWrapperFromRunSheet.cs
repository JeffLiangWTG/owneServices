using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.LocalCartage.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromRunSheet : FreightWrapper, IDocTypeCode
	{
		public FreightWrapperFromRunSheet(CommonWorkSheet runSheetBO, BusinessObjectFactory factory)
			: base(runSheetBO, factory)
		{
			RunSheetBO = runSheetBO ?? Factory.GetNull<CommonWorkSheet>();
		}
		readonly CommonWorkSheet RunSheetBO;

		#region Run Sheet Overrides

		protected override RunSheetWrapper GetRunSheet()
		{
			return new RunSheetWrapperFromCommonRunSheet(RunSheetBO, Factory);
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("2b5ce7d6-812f-4e9c-963b-9b40103e5c2a", "Run Sheet");
		}

		protected override ZString GetJobNumber()
		{
			return RunSheetBO.EY_RunSheetNumber;
		}

		protected override LocalTransportLegWrapperCollection GetLocalTransportLegs()
		{
			var result = new LocalTransportLegWrapperCollection(this, Factory, RunSheetBO.CartageLegs);
			result.Sort(LocalTransportLegWrapperCollection.SortBy.Sequence);
			return result;
		}

		#endregion

		#region IDocTypeCode Members

		ZString IDocTypeCode.DocTypeCode { get; set; }

		#endregion
	}
}
