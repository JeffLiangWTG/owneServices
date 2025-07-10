using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public abstract class EdiCommissionAgreementTreeBizObjWrapper : NonPersistentBusinessObject
	{
		protected EdiCommissionAgreementTreeBizObjWrapper(EdiCommissionAgreementCustomization customization, IEnumerable<EdiCommissionAgreementTreeBizObjWrapper> children)
			: base(customization.Factory)
		{
			this.customization = customization;
			this.Children = children;
		}

		protected readonly EdiCommissionAgreementCustomization customization;

		#region Selected

		public virtual ZBool Selected
		{
			get { return false; }
			set { }
		}

		public virtual ZBool Selected_Enabled
		{
			get { return false; }
		}

		#endregion

		#region Code

		public abstract ZString Code { get; }

		#endregion

		#region Description

		public abstract ZString Description { get; }

		#endregion

		#region ShouldAutoAdd

		public virtual ZBool ShouldAutoAdd
		{
			get { return false; }
			set { }
		}

		public virtual ZBool ShouldAutoAdd_Enabled
		{
			get { return false; }
		}

		#endregion

		#region IncludeDatabaseUsage

		public virtual ZBool IncludeDatabaseUsage
		{
			get { return false; }
			set { }
		}

		public virtual ZBool IncludeDatabaseUsage_Enabled
		{
			get { return false; }
		}

		#endregion

		public readonly IEnumerable<EdiCommissionAgreementTreeBizObjWrapper> Children;
	}
}
