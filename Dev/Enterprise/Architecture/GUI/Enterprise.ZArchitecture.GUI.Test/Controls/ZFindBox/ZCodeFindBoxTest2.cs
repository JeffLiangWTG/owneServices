namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCodeFindBoxTest2 : NUnit.Framework.TestCase
	{
		public void TestEnsureValidControlWidthDoesNotChangesWidthIfItIsZero()
		{
			FindBox.ShowDescriptionBox = false;
			FindBox.Width = 10;
			FindBox.EnsureValidControlWidth();
			Assert("Update width", FindBox.Width > 10);

			FindBox.Width = 0;
			FindBox.EnsureValidControlWidth();
			AssertEquals("Do not change width if it is 0", 0, FindBox.Width);
		}

		#region Implementation

		ZCodeFindBox FindBox
		{
			get { return findBox ?? (findBox = new ZCodeFindBox()); }
		}
		ZCodeFindBox findBox;

		protected override void TearDown()
		{
			base.TearDown();
			if (findBox != null)
			{
				findBox.Dispose();
			}
		}

		#endregion
	}
}
