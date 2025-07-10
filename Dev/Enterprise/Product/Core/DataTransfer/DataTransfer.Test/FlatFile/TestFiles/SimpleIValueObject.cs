using System.Collections.Specialized;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class SimpleIValueObject : ValueObject
	{
		public SimpleIValueObject()
		{
		}

		public ZString Value1
		{
			get { return fValue1; }
			set { fValue1 = value; }
		}

		public ZString Value2
		{
			get { return fValue2; }
			set { fValue2 = value; }
		}

		public StringCollection ValueCollection1
		{
			get
			{
				if (fValueCollection1 == null)
				{
					fValueCollection1 = new StringCollection();
				}
				return fValueCollection1;
			}
		}

		ZString fValue1;
		ZString fValue2;
		StringCollection fValueCollection1;
	}
}
