using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class EventReference : NonPersistentBusinessObject, IObsoleteValidation
	{
		public EventReference(string eventCode, string reference)
		{
			CompleteText = reference;
			this.eventCode = eventCode;
		}

		readonly string eventCode;

		#region Properties

		#region Parameter Collection

		public ParameterCollection ParameterCollection
		{
			get
			{
				if (parameterCollection == null)
				{
					parameterCollection = new ParameterCollection(this.eventCode);
					RegisterEditableChildObject(parameterCollection);
				}
				return parameterCollection;
			}
		}

		ParameterCollection parameterCollection;

		#endregion

		#region Reference Free Text (paremeterless)

		[MaxLength(StmALog.Schema.SL_ReferenceMaxLength)]
		public ZString FreeText
		{
			get { return freeText; }
			set { SetNonPersistentPropertyValue(FreeTextInfo, ref freeText, value); }
		}

		ZString freeText;

		public ZPropertyInfo FreeTextInfo
		{
			get { return GetZPropertyInfo(nameof(FreeText)); }
		}

		#endregion

		#region Generated Reference

		public ZString CompleteText
		{
			get { return GenerateReference(); }
			set { PopulateFreeTextAndParemeters(value); }
		}

		ZString GenerateReference()
		{
			return StmALog.GenerateEventReference(FreeText, GetParameterDictionary());
		}

		void PopulateFreeTextAndParemeters(ZString reference)
		{
			PopulateFreeText(reference);
			PopulateParameters(reference);
		}

		Dictionary<string, string> GetParameterDictionary()
		{
			return ParameterCollection
					.Cast<Parameter>()
					.ToDictionary(x => x.Code.ToString()
									 , x => x.ParamValue.ToString());
		}

		void PopulateFreeText(ZString reference)
		{
			FreeText = StmALog.GetFreeTextFromReference(reference);
		}

		void PopulateParameters(ZString reference)
		{
			ParameterCollection.RemoveAndDeleteAll();
			parameterCollection.AddRange(
				GetParameterCollection(StmALog.GetParametersFromReference(reference)
				));
		}

		IEnumerable<Parameter> GetParameterCollection(IEnumerable<KeyValuePair<string, string>> parameters)
		{
			return from parameter in parameters
				   select new Parameter(eventCode, parameter.Key, parameter.Value);
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateFreeText();
		}

		public void ValidateFreeText()
		{
			if (StmALog.EventReferenceSpecialCharacters.Any(c => FreeText.Contains(c)))
			{
				var msg = Res.GetString(
					"12fc4de0-554d-4485-af8a-a9ad9a1cf1eb",
					@"In your expression '{0}' has been recognized as a free text. 
If your intention is to enter this as a set of event parameters with values, you need to ensure the syntax is correct and the pipe character | is used at the start of each parameter.
A parameter code must not be longer than {1} characters.
Correct syntax is: Free Text|PAR=Value|PAR=Value etc.", freeText, Parameter.Schema.CodeMaxLength);

				FreeTextInfo.AddWarning(msg);
			}
		}

		#endregion

	}
}
