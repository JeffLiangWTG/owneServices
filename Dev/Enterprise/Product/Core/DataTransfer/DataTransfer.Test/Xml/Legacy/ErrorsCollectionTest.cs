using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class ErrorsCollectionTest : TestCase
	{
		public void TestToString()
		{
			Errors.Add("TEST");
			Errors.Add("Error");
			Errors.Add("ToString");

			AssertEquals("TEST\nError\nToString\n", Errors.ToString());
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Errors = new ErrorsCollection();
		}

		internal ErrorsCollection Errors;

		#endregion
	}
}
