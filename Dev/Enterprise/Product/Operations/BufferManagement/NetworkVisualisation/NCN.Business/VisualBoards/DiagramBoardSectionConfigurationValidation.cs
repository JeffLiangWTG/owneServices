using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace Enterprise.BufferManagement.Business
{
	public class DiagramBoardSectionConfigurationValidation : ZValidation
	{
		public DiagramBoardSectionConfigurationValidation(DiagramBoardSectionConfiguration sectionConfiguration)
			: base(sectionConfiguration)
		{
			this.Parent = sectionConfiguration;
		}

		#region DiagramPK

		public void ValidateDiagramPK()
		{
			ValidateCalculatedProperty(Parent.DiagramPKInfo);
		}

		protected void CheckDiagramPK()
		{
			MandatoryValidation.CheckEntered(Parent.DiagramPKInfo);
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			ValidateDiagramPK();
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(DiagramBoardSectionConfigurationValidation); }
		}

		readonly DiagramBoardSectionConfiguration Parent;

		#endregion

	}
}
