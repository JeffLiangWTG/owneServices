using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.Business.BillCustomisationStrategies
{
	internal class SequenceElementStrategy : ElementStrategy
	{
		public override string Key
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return BillOfLadingNumberCustomisationElement.Keys.SequenceNumber; }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = " Hard-coded constant")]
		public override string Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return "Sequence Number"; }
		}
		public override bool Force
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return true; }
		}
		public override byte DefaultOrder
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return 50; }
		}

		public override bool UseDetail
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return true; }
		}
		public override string DefaultDetail
		{
			get { return "8"; }
		}
		public override int DetailMaxLength
		{
			get { return 3; }
		}
		public override FieldType DetailFieldType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return FieldType.Integer; }
		}

		public override int CalcMaxGeneratedLength(BillOfLadingNumberCustomisationElement parent)
		{
			return ZInt.ParseSafe(parent.Detail, 8);
		}

		public override BillOfLadingNumberCustomisationElementValidation GetValidation(BillOfLadingNumberCustomisationElement parent)
		{
			return new BillOfLadingNumberCustomisationElementSequenceValidation(parent);
		}

		public override NumberCustomisationElementCategories Categories => NumberCustomisationElementCategories.ClientContract | base.Categories;

		/*
		 * Output example: [0-9]{4}
		 * Check the text is "number" and length is correct.
		 * 
		 * [0-9]{4}
		 * "9999" => true
		 * "1234" => true
		 * "99999" => false
		 * "a999" => false
		 * "999" => false
		 */
		public override string GetRegExForDataType(BillOfLadingNumberCustomisationElement parent)
			=> $"[0-9]{{{CalcMaxGeneratedLength(parent)}}}";
	}
}
