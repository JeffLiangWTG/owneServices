using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZListProviderFindBoxTest : BaseFindBoxTest
	{
		public void TestGetCustomCodeDescriptionCallsBaseList()
		{
			FindBox = NewFindBoxTester;
			FindBox.List = Dummy.Dummies;

			var provider = new Mock<IFindBoxListProvider>();
			var expected = new CodeDescriptionPair("BLAH", "Something that isnt the default");
			provider.Setup(p => p.GetCustomCodeDescription(It.IsAny<BusinessObject>())).Returns(expected);

			Dummy.Dummies.FindBoxListProviderOverride = provider.Object;

			AssertEquals(expected, ((IFindBoxListProvider)FindBox).GetCustomCodeDescription(Dummy.Dummies.AddNew()));
		}

		public void TestNearestMatch()
		{
			CreateDummies();

			FindBox = NewFindBoxTester;
			FindBox.List = Dummy.Dummies;

			AssertEquals("Nearest Match to A", "AABCD", ((IFindBoxListProvider)FindBox).NearestMatch("A", true, -1).Item1);
			AssertEquals("Nearest Match to AB", "ABCDE", ((IFindBoxListProvider)FindBox).NearestMatch("AB", true, -1).Item1);
			AssertEquals("Nearest Match to ABD", "ABDEF", ((IFindBoxListProvider)FindBox).NearestMatch("ABD", true, -1).Item1);
		}

		public void TestDescriptionFromCode()
		{
			CreateDummies();

			FindBox = NewFindBoxTester;
			FindBox.List = Dummy.Dummies;

			AssertEquals("Description of AABCD", "AABCD Description", ((IFindBoxListProvider)FindBox).DescriptionFromCode("AABCD"));
			AssertEquals("Description of ABCDE", "ABCDE Description", ((IFindBoxListProvider)FindBox).DescriptionFromCode("ABCDE"));
		}

		public void TestDescriptionFromPrimaryKey()
		{
			CreateDummies();

			FindBox = NewFindBoxTester;

			AssertNull("Description of AABCD", ((IFindBoxListProvider)FindBox).DescriptionFromPrimaryKey(Dummy1.PK));
			AssertNull("Description of ABCDE", ((IFindBoxListProvider)FindBox).DescriptionFromPrimaryKey(Dummy2.PK));
			AssertNull("Non-existent item", ((IFindBoxListProvider)FindBox).DescriptionFromPrimaryKey(ZGuid.NewZGuid()));
			AssertNull("Invalid PK", ((IFindBoxListProvider)FindBox).DescriptionFromPrimaryKey(ZGuid.Invalid));

			FindBox.List = Dummy.Dummies;

			AssertEquals("Description of AABCD", "AABCD Description", ((IFindBoxListProvider)FindBox).DescriptionFromPrimaryKey(Dummy1.PK));
			AssertEquals("Description of ABCDE", "ABCDE Description", ((IFindBoxListProvider)FindBox).DescriptionFromPrimaryKey(Dummy2.PK));
			AssertNull("Non-existent item", ((IFindBoxListProvider)FindBox).DescriptionFromPrimaryKey(ZGuid.NewZGuid()));
			AssertNull("Invalid PK", ((IFindBoxListProvider)FindBox).DescriptionFromPrimaryKey(ZGuid.Invalid));
		}

		public void TestList()
		{
			var list = Factory.GetCachedValue<CodeDescriptionPairList>();

			FindBox = NewFindBoxTester;
			FindBox.List = list;

			IBusinessObjectCollection newList = null;
			AssertNoExceptionThrown(delegate
			{
				newList = ((IFindBoxListProvider)FindBox).List;
			});

			AssertNull(newList);
		}

		#region Implementation

		protected override ZFindBoxUserControl NewFindBoxTester
		{
			get { return new ZListProviderFindBoxTester(); }
		}

		class ZListProviderFindBoxTester : ZListProviderFindBox
		{
			protected internal override IFindBoxPopup PopupForm
			{
				get { return null; }
			}
		}

		#endregion
	}
}
