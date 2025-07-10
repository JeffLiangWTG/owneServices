using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIGlbStaffControllerOverride : GlbStaffController
	{
		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			return new EdiGlbStaffForm((EDIGlbStaff)businessEntity);
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EDIGlbStaff); }
		}
	}
}
