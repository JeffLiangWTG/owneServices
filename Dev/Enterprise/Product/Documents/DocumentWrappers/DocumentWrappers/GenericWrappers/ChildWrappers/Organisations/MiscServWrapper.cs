using System;
using System.Drawing;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <summary>
	/// Generic MiscServ BO helper Wrapper to be conusmed by the OrganisationWrapper.
	/// </summary>
	/// <remarks>
	/// This class is intentionally flagged internal to simplify field naming in the template.
	/// </remarks>
	internal class MiscServWrapper : GenericWrapper
	{
		public MiscServWrapper(OrgMiscServ orgMiscServ, BusinessObjectFactory factory)
			: base(orgMiscServ, factory)
		{
		}

		/// <summary>
		/// Dangerous Goods Contact
		/// </summary>
		internal DGContactWrapper DGContact
		{
			get
			{
				if (OrgMiscServBO != null && dangerousGoodsContact == null)
				{
					dangerousGoodsContact = new DGContactWrapper(OrgMiscServBO, Factory);
				}
				return dangerousGoodsContact;
			}
		}
		DGContactWrapper dangerousGoodsContact;

		internal PartAttributeWrapper PartAttribute1
		{
			get
			{
				if (OrgMiscServBO != null && partAttribute1 == null)
				{
					partAttribute1 = new PartAttributeWrapper(OrgMiscServBO, 1, Factory);
				}
				return partAttribute1;
			}
		}
		PartAttributeWrapper partAttribute1;

		internal PartAttributeWrapper PartAttribute2
		{
			get
			{
				if (OrgMiscServBO != null && partAttribute2 == null)
				{
					partAttribute2 = new PartAttributeWrapper(OrgMiscServBO, 2, Factory);
				}
				return partAttribute2;
			}
		}
		PartAttributeWrapper partAttribute2;

		internal PartAttributeWrapper PartAttribute3
		{
			get
			{
				if (OrgMiscServBO != null && partAttribute3 == null)
				{
					partAttribute3 = new PartAttributeWrapper(OrgMiscServBO, 3, Factory);
				}
				return partAttribute3;
			}
		}
		PartAttributeWrapper partAttribute3;

		internal Image ClientDocumentLogo
		{
			get
			{
				Image result = null;
				if (OrgMiscServBO != null && OrgMiscServBO.ClientDocumentLogo.Length > 0)
				{
					try
					{
						MemoryStream stream = new MemoryStream(OrgMiscServBO.ClientDocumentLogo);
						result = Image.FromStream(stream);
					}
					catch (Exception exception)
					{
						if (exception.IsCriticalException())
						{ throw; }
						result = null;
					}
				}
				return result;
			}
		}

		OrgMiscServ OrgMiscServBO
		{
			get { return orgMiscServBO ?? (orgMiscServBO = (OrgMiscServ)WrappedBO); }
		}
		OrgMiscServ orgMiscServBO;
	}
}
