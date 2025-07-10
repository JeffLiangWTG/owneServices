using System.Collections.Generic;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CompanyCredentialsLayout))]
	sealed class CompanyCredentialsLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CompanyCredentialsControlBag.Instance.UnipassCertificateGroupBox, ControlWidthClass.LongControl);
				yield return (MasterFiles.GUI.CompanyCredentialsControlBag.Instance.ICS2CredentialUserControl, ControlWidthClass.LongControl);
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CompanyCredentialsLayoutBuilder<GlbCompanyWrapper>();
	}
}
