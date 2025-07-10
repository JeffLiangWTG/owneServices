using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class InterchangeSenderProxyUserCollection : RegistryBusinessObjectCollection, ICodeDescriptionPairList
	{
		public InterchangeSenderProxyUserCollection()
		{
		}

		public InterchangeSenderProxyUserCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public IGlbStaff GetStaffFromRecipient(ZString recipientCode, BusinessObjectFactory factory)
		{
			var pair = (InterchangeSenderProxyUser)FindByCode(recipientCode);

			IGlbStaff result = null;
			if (!string.IsNullOrEmpty(pair?.DescriptionValue))
			{
				result = factory.LoadTop1<IGlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, pair.DescriptionValue).AddToFilter(GlbStaffSchema.GS_IsActive, true));
			}

			return result ?? factory.LoadTop1<IGlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, User.InterchangeUserCode));
		}

		public new InterchangeSenderProxyUser this[int i] => (InterchangeSenderProxyUser)base[i];

		public new InterchangeSenderProxyUser AddNew() => (InterchangeSenderProxyUser)base.AddNew();

		protected override bool IgnoreCaseInCodes => true;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new InterchangeSenderProxyUser(CurrentFactory);
		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new InterchangeSenderProxyUserCollection(factory);

		#region ICodeDescriptionPairList impl

		bool ICodeDescriptionPairList.ContainsCode(object code) => FindByCode((string)code) != null;
		string ICodeDescriptionPairList.GetDescriptionFromCode(string code) => FindByCode(code)?.Description;

		#endregion
	}
}
