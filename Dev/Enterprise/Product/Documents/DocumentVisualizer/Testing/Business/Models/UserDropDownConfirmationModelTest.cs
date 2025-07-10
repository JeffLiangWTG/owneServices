using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Models;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(UserDropDownConfirmationModel))]
	sealed class UserDropDownConfirmationModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateOption()
		{
			var model = (UserDropDownConfirmationModel)GetNewBusinessObject();

			model.Option = "!?!";
			AssertHasError(model.OptionInfo, "Enter a valid selection.");

			model.Option = "aaa";
			AssertNoErrors(model.OptionInfo);

			model.Option = "";
			AssertHasError(model.OptionInfo, "Please enter a value.");

			model.Option = "bbb";
			AssertNoErrors(model.OptionInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("aaa", "aaa desc");
			list.AddPair("bbb", "bbb desc");
			list.AddPair("ccc", "ccc desc");

			return new UserDropDownConfirmationModel(list);
		}
	}
}
