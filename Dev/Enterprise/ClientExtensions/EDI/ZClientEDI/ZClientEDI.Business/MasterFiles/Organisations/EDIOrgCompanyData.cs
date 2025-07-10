using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgCompanyData : OrgCompanyData
	{
		public EDIOrgCompanyData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		#region Header

		EDIOrgHeader fHeader;
		public new EDIOrgHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = Factory.Load<EDIOrgHeader>(OB_OH);
				}
				return fHeader;
			}
		}

		#endregion

		public override ZBool OB_ARWHTApplicable
		{
			get { return base.OB_ARWHTApplicable; }
			set
			{
				base.OB_ARWHTApplicable = value;
				if (Header.LicCompany != null)
				{
					Header.LicCompany.LC_IsWHTRegistered = base.OB_ARWHTApplicable;
				}
			}
		}

		public override ZBool OB_IsCreditor
		{
			get { return base.OB_IsCreditor; }
			set
			{
				base.OB_IsCreditor = value;
				if (Header != null)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool OB_IsDebtor
		{
			get { return base.OB_IsDebtor; }
			set
			{
				base.OB_IsDebtor = value;
				if (Header != null)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		#endregion
	}
}

