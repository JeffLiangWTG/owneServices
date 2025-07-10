using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class SendCancellationMessageDataObjectValidation : AutoSendCancellationMessageDataObjectValidation
	{
		public SendCancellationMessageDataObjectValidation(AutoSendCancellationMessageDataObject parent) : base(parent)
		{
		}

		protected override void CheckInvalidationMotivation()
		{
			MandatoryValidation.CheckEntered(Parent.InvalidationMotivationInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.InvalidationMotivationInfo);
		}

		void ValidateTargets()
		{
			Parent.ClearRowNotifications();
			if (Parent.Targets.Length > Parent.MaxCount())
			{
				Parent.AddRowError($"The number of targets exceeds maximum allowed of {Parent.MaxCount()} items.");
				return;
			}

			foreach (var target in Parent.Targets)
			{
				if (target is JobDeclaration declaration)
				{
					if (declaration.JE_ApplicationCode != DeclarationApplicationCodeList.Codes.DeltaIE)
					{
						Parent.AddRowError($"Declaration {declaration.JE_DeclarationReference} is not Delta I/E.");
					}

					if (declaration.CustomsEntryHeaders.Count == 0)
					{
						Parent.AddRowError($"Declaration {declaration.JE_DeclarationReference} doesn't have entries.");
					}

					foreach (var header in declaration.CustomsEntryHeaders)
					{
						ValidateMandatoryField(header, header.CorrelationIDInfo);
						ValidateMandatoryField(header, header.MovementReferenceNumberInfo);
						ValidateMandatoryField(header, header.CRNInfo);
					}
				}
				else if (target is CusEntryHeader cusEntryHeader)
				{
					if (cusEntryHeader.Declaration.JE_ApplicationCode != DeclarationApplicationCodeList.Codes.DeltaIE)
					{
						Parent.AddRowError($"Declaration {cusEntryHeader.Declaration.JE_DeclarationReference} of the entry {cusEntryHeader.CH_BGMReference} is not Delta I/E.");
					}

					ValidateMandatoryField(cusEntryHeader, cusEntryHeader.CorrelationIDInfo);
					ValidateMandatoryField(cusEntryHeader, cusEntryHeader.MRNInfo);
					ValidateMandatoryField(cusEntryHeader, cusEntryHeader.CRNInfo);
				}
			}

			void ValidateMandatoryField(CusEntryHeader header, ZPropertyInfo propertyInfo)
			{
				if (propertyInfo.Value.IsEmpty)
				{
					var errorField = propertyInfo.HumanReadableName;
					var notification = MandatoryValidation.MustBeEnteredMessage(errorField);
					Parent.AddRowError($"Field {errorField} of the entry {header.CH_BGMReference} has error '{notification}'.");
				}
			}
		}

		internal bool AreTargetsValid()
		{
			ValidateTargets();

			return !Parent.HasErrors;
		}

		public new SendCancellationMessageDataObject Parent => (SendCancellationMessageDataObject)base.Parent;
	}
}
