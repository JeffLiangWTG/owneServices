using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationLoadingCompletionMessageSendingObject : JobDeclarationMessageSendingObject
	{
		public JobDeclarationLoadingCompletionMessageSendingObject(CusEntryHeader entry, ZString messageType)
			: base(entry, messageType)
		{
		}

		public new JobDeclarationLoadingCompletionMessageSendingObjectValidation Validation => (JobDeclarationLoadingCompletionMessageSendingObjectValidation)base.Validation;
		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new JobDeclarationLoadingCompletionMessageSendingObjectValidation(this);

		[ResourceStringData("JobDeclarationLoadingCompletionMessageSendingObject|CustomsReceiptNumber", Caption = "Customs Receipt Number")]
		public ZString CustomsReceiptNumber => Header.FormattedRefNumber;
		[ResourceStringData("JobDeclarationLoadingCompletionMessageSendingObject|CustomsOffice", Caption = "Customs Office")]
		public ZString CustomsOffice => Header.Declaration.JE_CustomsOffice;
		[ResourceStringData("JobDeclarationLoadingCompletionMessageSendingObject|Department", Caption = "Department")]
		public ZString Department => Header.Declaration.JE_CustomsDivision;
		[ResourceStringData("JobDeclarationLoadingCompletionMessageSendingObject|StevedoresCompany", Caption = "Stevedores Company")]
		public ZString StevedoresCompany => Header.Declaration.StevedoreCompany.CompanyName;
		[ResourceStringData("JobDeclarationLoadingCompletionMessageSendingObject|LoadingDate", Caption = "Loading Date")]

		public ZDateTime LoadingDate
		{
			get
			{
				return Header.Declaration.JE_EntryDate;
			}
			set
			{
				Header.Declaration.JE_EntryDate = (ZDate)value;
				LoadingDateInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateLoadingDate();
				}
			}
		}
		public ZPropertyInfo LoadingDateInfo => GetZPropertyInfo(nameof(LoadingDate));

		public CusPersonCollection Stevedores
		{
			get
			{
				if (stevedores == null)
				{
					stevedores = Header.Declaration.Persons;
				}
				return stevedores;
			}
		}

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				base.ShouldSend = value;
				if (value)
				{
					if (Header.Declaration.IsLoadingDateRelevant && LoadingDate.IsEmpty)
					{
						LoadingDate = ZDateTime.Today;
					}
				}
			}
		}

		CusPersonCollection stevedores;
	}
}
