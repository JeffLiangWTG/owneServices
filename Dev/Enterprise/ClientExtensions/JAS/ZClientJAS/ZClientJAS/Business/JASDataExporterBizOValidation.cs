using System;
using System.IO;
using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business
{
	public class JASDataExporterBizOValidation : ZValidation
	{
		public JASDataExporterBizOValidation(JASDataExporterBizO parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public override Type AutoValidationType
		{
			get { return typeof(JASDataExporterBizOValidation); }
		}

		public override void ValidateAll()
		{
			ValidateDeliveryMethod();
			ValidateExportDirectory();
			ValidateEmailAddress();
			ValidateEmailGroupPK();
		}

		#region Delivery Method

		public void ValidateDeliveryMethod()
		{
			ValidateCalculatedProperty(Parent.DeliveryMethodInfo);
		}

		protected void CheckDeliveryMethod()
		{
			MandatoryValidation.CheckEntered(Parent.DeliveryMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DeliveryMethodInfo, Parent.DeliveryMethodList);
		}

		#endregion

		#region Export Directory

		public void ValidateExportDirectory()
		{
			ValidateCalculatedProperty(Parent.ExportDirectoryInfo);
		}

		protected void CheckExportDirectory()
		{
			if (Parent.IsDirectoryDeliveryMethod)
			{
				MandatoryValidation.CheckEntered(Parent.ExportDirectoryInfo, "Export Directory");
				if (!Parent.ExportDirectory.IsEmpty && !Directory.Exists(Parent.ExportDirectory))
				{
					string message = string.Format("Directory \"{0}\" does not exist. Please select a different directory.", Parent.ExportDirectory);
					Parent.ExportDirectoryInfo.AddError(message);
				}
			}
		}

		#endregion

		#region Email Address

		public void ValidateEmailAddress()
		{
			ValidateCalculatedProperty(Parent.EmailAddressInfo);
		}

		protected void CheckEmailAddress()
		{
			if (Parent.IsEmailDeliveryMethod && Parent.IsIndividualEmailRecipient)
			{
				const string PropertyDescription = "Recipient Email Address";
				MandatoryValidation.CheckEntered(Parent.EmailAddressInfo, PropertyDescription);

				if (!Parent.EmailAddress.IsEmpty)
				{
					EmailAddressValidation.ValidateEmailAddress(Parent.EmailAddressInfo, PropertyDescription);
				}
			}
		}

		#endregion

		#region Email Group

		public void ValidateEmailGroupPK()
		{
			ValidateCalculatedProperty(Parent.EmailGroupPKInfo);
		}

		protected void CheckEmailGroupPK()
		{
			if (Parent.IsEmailDeliveryMethod && Parent.IsGroupEmailRecipient)
			{
				MandatoryValidation.CheckEntered(Parent.EmailGroupPKInfo);
				ListValidation.ErrorIfInvalidPK(Parent.EmailGroupPKInfo, Parent.EmailGroups);
			}
		}

		#endregion

		public readonly JASDataExporterBizO Parent;
	}
}
