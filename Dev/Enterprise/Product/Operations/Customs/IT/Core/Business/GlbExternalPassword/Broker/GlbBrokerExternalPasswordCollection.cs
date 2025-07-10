using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business;

public class GlbBrokerExternalPasswordCollection : DependentBusinessObjectCollection<GlbBrokerExternalPassword, GlbStaff>, MasterFiles.Integration.CustomsIntegration.IT.IGlbBrokerExternalPasswordCollection
{
	public GlbBrokerExternalPasswordCollection(GlbStaff staff) : base(staff, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.ITB))
	{
	}

	protected override void OnAdded(BusinessObject bizOAdded)
	{
		if (!IsLoading && !IsCertificateInfoPropagationSuspended)
		{
			using (SuspendCertificateInfoPropagation())
			{
				var targetObject = (GlbExternalPassword)bizOAdded;
				var sourceObject = GetExistingExternalPasswordsExceptMe(targetObject).FirstOrDefault();
				if (sourceObject != null)
				{
					CopyValueToTargetPropertyInfo(targetObject, sourceObject.GP_CertificateInfo, sourceObject.CurrentDecryptedCertificatePassphraseInfo);
				}
			}
		}
		base.OnAdded(bizOAdded);
	}

	public void CopyCertificateInfo(GlbExternalPassword sourceObject, ZPropertyInfo sourcePropertyInfo)
	{
		if (!IsLoading && !IsCertificateInfoPropagationSuspended)
		{
			using (SuspendCertificateInfoPropagation())
			{
				foreach (var targetObject in GetExistingExternalPasswordsExceptMe(sourceObject))
				{
					CopyValueToTargetPropertyInfo(targetObject, sourcePropertyInfo);
				}
			}
		}
	}

	void CopyValueToTargetPropertyInfo(GlbExternalPassword targetObject, params ZPropertyInfo[] sourcePropertyInfoList)
	{
		foreach (var sourcePropertyInfo in sourcePropertyInfoList)
		{
			var sourcePropertyInfoName = sourcePropertyInfo.Name;
			var targetPropertyInfo = targetObject.FindPropertyInfo(sourcePropertyInfoName)
				?? throw new InvalidOperationException($"Attemp to change the value of a property that does not belong to {nameof(GlbExternalPassword)}: {sourcePropertyInfoName}.");
			targetPropertyInfo.Value = sourcePropertyInfo.Value;
		}
	}

	IEnumerable<GlbExternalPassword> GetExistingExternalPasswordsExceptMe(GlbExternalPassword sourceObject) => this.Cast<GlbExternalPassword>().Where(x => x.PK != sourceObject.PK);

	#region Certificate Info Propagation Suspender

	bool IsCertificateInfoPropagationSuspended => certificateInfoPropagationSuspenderIndex > 0;

	int certificateInfoPropagationSuspenderIndex;

#if DEBUG

	public
#endif
	IDisposable SuspendCertificateInfoPropagation()
	{
		certificateInfoPropagationSuspenderIndex++;
		return new DisposableAction(() => certificateInfoPropagationSuspenderIndex--);
	}

	#endregion
}
