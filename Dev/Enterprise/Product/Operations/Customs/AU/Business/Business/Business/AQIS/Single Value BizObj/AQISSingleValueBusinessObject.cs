
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class AQISSingleValueBusinessObject : NonPersistentBusinessObject, IAQISUniqueCodeForSort
	{
		public AQISSingleValueBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Code

		public ZString Code
		{
			get { return fCode; }
			set
			{
				if (fCode != value)
				{
					CheckMaximumLength(CodeInfo, value);
					SetNonPersistentPropertyValue(CodeInfo, ref fCode, value);
					Validation.ValidateCode();
				}
			}
		}
		ZString fCode;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(nameof(Code)); }
		}

		protected abstract int Code_MaxLength { get; }

		#endregion

		public virtual AQISSingleValueValidation Validation
		{
			get { return null; }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#region IAQISUniqueCodeForSort Members

		ZString[] IAQISUniqueCodeForSort.CodesToSortBy
		{
			get
			{
				return new ZString[] { Code };
			}
		}

#if DEBUG

		ZPropertyInfo[] IAQISUniqueCodeForSort.CodeInfosToSortByForTestingOnly
		{
			get
			{
				return new ZPropertyInfo[] { CodeInfo };
			}
		}

#endif

		#endregion
	}
}
