using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Integration.CustomsIntegration.IT;

namespace Enterprise.Customs.IT.Business;

public class GlbStaffWrapper : MasterFiles.Business.GlbStaffWrapper, IGlbStaffWrapper
{
	protected GlbStaffWrapper(GlbStaff staff)
		: base(staff)
	{
	}

	public static GlbStaffWrapper Get(GlbStaff staff)
	{
		return staff?.Factory.GetCachedValue(staff.PK.ToString(), () => new GlbStaffWrapper(staff));
	}

	#region PasswordCollection

	[ChildEditable]
	public GlbBrokerExternalPasswordCollection PasswordCollection
	{
		get
		{
			if (passwordCollection == null)
			{
				passwordCollection = new GlbBrokerExternalPasswordCollection(Staff);
				passwordCollection.Load();
				RegisterEditableChildObject(passwordCollection);
			}

			return passwordCollection;
		}
	}
	GlbBrokerExternalPasswordCollection passwordCollection;

	IGlbBrokerExternalPasswordCollection IGlbStaffWrapper.PasswordCollection => PasswordCollection;

	#endregion

	#region CryptokiCertificate

	public CryptokiExternalPasswordCollection CryptokiCertificateCollection
	{
		get
		{
			if (cryptokiCertificateCollection == null)
			{
				cryptokiCertificateCollection = new CryptokiExternalPasswordCollection(Staff);
				cryptokiCertificateCollection.Load();
				RegisterEditableChildObject(cryptokiCertificateCollection);
			}

			return cryptokiCertificateCollection;
		}
	}
	CryptokiExternalPasswordCollection cryptokiCertificateCollection;

	#endregion

	#region AutomaticSignaturePasswordCollection

	public AutomaticSignatureExternalPasswordCollection AutomaticSignaturePasswordCollection
	{
		get
		{
			if (automaticSignaturePasswordCollection == null)
			{
				automaticSignaturePasswordCollection = new AutomaticSignatureExternalPasswordCollection(Staff);
				automaticSignaturePasswordCollection.Load();
				RegisterEditableChildObject(automaticSignaturePasswordCollection);
			}

			return automaticSignaturePasswordCollection;
		}
	}

	AutomaticSignatureExternalPasswordCollection automaticSignaturePasswordCollection;

	#endregion
}
