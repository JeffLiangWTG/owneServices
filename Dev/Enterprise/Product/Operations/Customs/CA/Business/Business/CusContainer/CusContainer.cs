namespace Enterprise.Customs.CA.Business
{
	using System.Data;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageBuilders;

	public partial class CusContainer : AutoCusContainer, IG7Container
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public override ZGuid CO_RC
		{
			get { return base.CO_RC; }
			set
			{
				base.CO_RC = value;
				if (Container != null && CA_ContainerSizeOrISOCode.IsEmpty)
				{
					CA_ContainerSizeOrISOCode = Container.RC_ISOType;
				}
			}
		}

		#region Implementation of IG7Container

		ZString IG7Container.ContainerNumber
		{
			get { return CO_ContainerNumber; }
		}

		ZString IG7Container.CountryOfRegistration
		{
			get { return CA_RN_NKCountryOfRegistration; }
		}

		ZString IG7Container.ContainerSizeCode
		{
			get { return CA_ContainerSizeOrISOCode; }
		}

		#endregion
	}
}
