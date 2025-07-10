using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Common.GUI.Testing
{
	class DummyChildWithInterface : DummyChildEnterpriseBusinessObject, IHaveAdditionalDataForBorderWise
	{
		public DummyChildWithInterface(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
		{
		}

		public AdditionalDataForBorderWise GetAdditionalDataForBorderWise(string bindTo)
		{
			return new AdditionalDataForBorderWise(Z0_Bool ? "I" : "E", Z0_Date);
		}

		public Type ExpectedBusinessObjectTypeForList
		{
			get
			{
				return null;
			}
		}
	}
}
