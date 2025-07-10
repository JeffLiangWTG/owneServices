using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(FaxPrice))]
	internal sealed class FaxPriceTest : RegistryBusinessObjectTemplateTestCase<FaxPrice>
	{
		public void TestValidateCode()
		{
			FaxPriceCollection list = new FaxPriceCollection();
			FaxPrice item1 = list.AddNew();
			AssertNoNotifications(item1.CodeInfo);
			item1.Code = "";
			AssertHasErrors(item1.CodeInfo);
			item1.Code = "XXX";
			AssertHasErrors(item1.CodeInfo);
			item1.Code = "AUD";
			AssertNoNotifications(item1.CodeInfo);

			FaxPrice item2 = list.AddNew();
			item2.Code = "AUD";
			AssertHasError(item2.CodeInfo, "The Code has been duplicated and must be unique.");
			item2.Code = "NZD";
			AssertNoNotifications(item2.CodeInfo);
		}

		public void TestValidatePrice()
		{
			FaxPriceCollection list = new FaxPriceCollection();
			FaxPrice item = list.AddNew();
			item.Price = 0;
			AssertHasErrors(item.PriceInfo);
			item.Price = -1;
			AssertHasErrors(item.PriceInfo);
			item.Price = 1;
			AssertNoNotifications(item.PriceInfo);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override FaxPrice GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override FaxPrice GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		FaxPrice NewPopulatedBusinessObject()
		{
			FaxPrice result = new FaxPrice();
			result.Code = "AUD";
			result.Price = 0.20m;
			return result;
		}

		#endregion
	}
}
