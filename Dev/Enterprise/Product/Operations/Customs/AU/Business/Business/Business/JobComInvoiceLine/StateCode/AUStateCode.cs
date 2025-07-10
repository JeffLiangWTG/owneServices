using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUStateCode : NonPersistentBusinessObject
	{
		#region Schema
		public abstract class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
			public const int CodeLength = 5;
		}
		#endregion

		public AUStateCode(JobComInvoiceLine invoiceLine, BusinessObjectFactory factory)
		: base(factory)
		{
			this.invoiceLine = invoiceLine;
		}

		public JobComInvoiceLine Parent
		{
			get { return invoiceLine; }
		}

		readonly JobComInvoiceLine invoiceLine;

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
			get { return GetZPropertyInfo(Schema.Code); }
		}

		public ZString Description
		{
			get
			{
				return Lookups.AUStateCodeList.GetDescriptionFromCode(Code);
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		protected int Code_MaxLength
		{
			get { return Schema.CodeLength; }
		}

		#region Validation

		public AUStateCodeValidation Validation
		{
			get { return new AUStateCodeValidation(this); }
		}

		#endregion

		public AUStateCodeLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new AUStateCodeLookups(this);
				}
				return fLookups;
			}
		}
		AUStateCodeLookups fLookups;
	}
}
