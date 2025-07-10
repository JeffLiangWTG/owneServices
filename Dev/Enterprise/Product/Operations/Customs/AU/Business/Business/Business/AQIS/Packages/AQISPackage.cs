using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISPackage : NonPersistentBusinessObject, IAQISUniqueCodeForSort
	{
		public AQISPackage(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Number

		public ZInt Number
		{
			get { return fNumber; }
			set
			{
				if (fNumber != value)
				{
					SetNonPersistentPropertyValue(NumberInfo, ref fNumber, value);
					Validation.ValidateNumber();
				}
			}
		}
		ZInt fNumber;

		public ZPropertyInfo NumberInfo
		{
			get { return GetZPropertyInfo(nameof(Number)); }
		}

		#endregion

		#region Type

		[CargoWise.ComponentModel.MaxLength(4)]
		public ZString Type
		{
			get { return fType; }
			set
			{
				if (fType != value)
				{
					CheckMaximumLength(TypeInfo, value);
					SetNonPersistentPropertyValue(TypeInfo, ref fType, value);
					Validation.ValidateType();
				}
			}
		}
		ZString fType;

		public ZPropertyInfo TypeInfo
		{
			get { return GetZPropertyInfo(nameof(Type)); }
		}

		#endregion

		#region Lookups

		public AQISPackageLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new AQISPackageLookups(this);
				}

				return fLookups;
			}
		}
		AQISPackageLookups fLookups;

		#endregion

		#region Validation

		public AQISPackageValidation Validation
		{
			get { return new AQISPackageValidation(this); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region IAQISUniqueCodeForSort Members

		ZString[] IAQISUniqueCodeForSort.CodesToSortBy
		{
			get
			{
				return new ZString[] { Type };
			}
		}

#if DEBUG

		ZPropertyInfo[] IAQISUniqueCodeForSort.CodeInfosToSortByForTestingOnly
		{
			get
			{
				return new ZPropertyInfo[] { TypeInfo };
			}
		}

#endif
		#endregion
	}
}
