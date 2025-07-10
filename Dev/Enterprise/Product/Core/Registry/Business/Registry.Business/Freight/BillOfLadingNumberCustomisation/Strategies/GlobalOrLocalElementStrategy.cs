using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.BillCustomisationStrategies
{
	internal class GlobalOrLocalElementStrategy : ElementStrategy
	{
		public override string Key
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return BillOfLadingNumberCustomisationElement.Keys.GlobalOrLocal; }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Hard-coded constant")]
		public override string Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return "Global Or Local"; }
		}

		public override string Description
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (NoResString)"Global = G, Local = L"; }
		}

		public override bool ReadOnly
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return true; }
		}

		public override FieldType DetailFieldType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return FieldType.Text; }
		}

		public override int CalcMaxGeneratedLength(BillOfLadingNumberCustomisationElement parent)
		{
			return ZInt.ParseSafe(parent.Detail, 1);
		}

		public override NumberCustomisationElementCategories Categories => NumberCustomisationElementCategories.ClientContract;

		/*
		 * Output: [OL]{1}
		 * Check the text is "O or L"
		 * 
		 * [OL]{1}
		 * "O" => true
		 * "L" => true
		 * "o" => false
		 * "l" => false
		 * "9" => false
		 * "OL" => false
		 * "A" => false
		 * "" => false
		 */
		public override string GetRegExForDataType(BillOfLadingNumberCustomisationElement parent)
			=> (NoResString)"[OL]{1}";
	}
}
