using System;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.JXC.Export.Validations;
using Enterprise.Freight.Forwarding.AWB.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.AWB.Testing
{
	[TestedType(typeof(JASNatureAndQtyOfGoods))]
	public class JASNatureAndQtyOfGoodsTest : NatureAndQtyOfGoodsTest
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Nature and Quantity of Goods", GetNewBusinessObject().HumanReadableName);
		}

		public override Type ExpectedValidationType
		{
			get
			{
				return typeof(JXCNatureAndQtyOfGoodsValidation);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JASNatureAndQtyOfGoods(Factory.New<JASConsolExportAWBRateLine>());
		}
	}
}
