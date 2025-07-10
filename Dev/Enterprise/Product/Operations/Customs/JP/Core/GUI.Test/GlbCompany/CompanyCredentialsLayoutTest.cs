using System.Collections.Generic;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CompanyCredentialsLayout))]
	sealed class CompanyCredentialsLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CompanyCredentialsLayoutBuilder<JPGlbCompanyWrapper>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (MasterFiles.GUI.CompanyCredentialsControlBag.Instance.ICS2CredentialUserControl, ControlWidthClass.LongControl);
				yield return (CompanyCredentialsControlBag.Instance.NaccsMailboxGroupBox, ControlWidthClass.LongControl);
			}
		}
	}
}
