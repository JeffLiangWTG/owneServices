using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.AWB.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	[TestedType(typeof(JXCNatureAndQtyOfGoodsValidation))]
	public class JXCNatureAndQtyOfGoodsValidationTest : NatureAndQtyOfGoodsValidationTest
	{
		protected override void AssertText()
		{
			base.AssertText();
			NatureAndQtyOfGoods parent = GetNewParent();
			parent.Validation.ValidateText();
			AssertHasWarning("JAS specific warning", parent.TextInfo, "JXC: You have not entered a value.");
			parent.Text = "XXX";
			parent.Validation.ValidateText();
			AssertNoWarning("JAS specific warning", parent.TextInfo, "JXC: You have not entered a value.");
		}

		protected override NatureAndQtyOfGoodsValidation GetNewValidation(NatureAndQtyOfGoods parent)
		{
			return new JXCNatureAndQtyOfGoodsValidation((JASNatureAndQtyOfGoods)parent);
		}

		protected override NatureAndQtyOfGoods GetNewParent()
		{
			return new JASNatureAndQtyOfGoods(Factory.New<JASConsolExportAWBRateLine>());
		}
	}
}
