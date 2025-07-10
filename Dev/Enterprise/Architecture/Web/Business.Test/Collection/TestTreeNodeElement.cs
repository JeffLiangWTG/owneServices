using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class TestTreeNodeElement : IFamilyMember
	{
		public TestTreeNodeElement(string shortDescription, string longDescription, string quantity)
		{
			fShortDescription = shortDescription;
			fLongDescription = longDescription;
			fQuantity = quantity;
		}

		#region IFamilyMember Members

		public bool HasChildren
		{
			get
			{
				return false;
			}
		}

		public IFamilyMember[] Children
		{
			get
			{
				return System.Array.Empty<TestTreeNodeElement>();
			}
		}

		public ZPropertyInfo LongDescriptionInfo
		{
			get
			{
				return null;
			}
		}

		readonly string fShortDescription;

		public string ShortDescription
		{
			get { return fShortDescription; }
		}

		readonly ZString fLongDescription;
		public ZString LongDescription
		{
			get { return fLongDescription; }
		}

		readonly ZString fQuantity;

		public ZString Quantity
		{
			get { return fQuantity; }
		}
		#endregion
	}
}
