using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISConcernTypeValidation : AQISSingleValueValidation
	{
		public AQISConcernTypeValidation(AQISConcernType parent)
			: base(parent)
		{
			this.aQISConcernType = parent;
		}

		public override Type AutoValidationType
		{
			get { return typeof(AQISConcernTypeValidation); }
		}

		#region Code

		public override void ValidateCodeAgainstLookupList()
		{
			ListValidation.MessageErrorIfInvalidCode(aQISConcernType.CodeInfo, aQISConcernType.Lookups.AQISConcernTypeList);
		}

		public override ZInt MaximumNumberOfRecordsAllowed
		{
			get { return 6; }
		}

		protected internal override char[] CharsNotAllowed
		{
			get { return new char[2] { ',', '/' }; }
		}

		protected override void CheckCode()
		{
			base.CheckCode();

			if (!aQISConcernType.IsValidationSuspended)
			{
				if (aQISConcernType.ParentCollections.Count > 0)
				{
					AQISConcernTypeCollection concernTypes = ((AQISConcernTypeCollection)aQISConcernType.ParentCollections.First());

					foreach (AQISConcernType currentConcernType in concernTypes)
					{
						if (currentConcernType.Code != "" && currentConcernType != aQISConcernType && currentConcernType.Code == aQISConcernType.Code)
						{
							aQISConcernType.CodeInfo.AddMessageError("You cannot repeat Concern Types. Please select different Concern Type.");
							break;
						}
					}
					JobDeclaration declaration = concernTypes.Declaration;
					if (declaration != null && !declaration.IsValidationSuspended)
					{
						declaration.ValidateAllCPDecQuestions();
						if (declaration.AddInfo.Validation.GetType() == typeof(CMRAddInfoDeclarationValidation))
						{
							((CMRAddInfoDeclarationValidation)declaration.AddInfo.Validation).ValidateZA_AQISInspectLocation_Hidden();
						}
					}
				}
			}
		}

		#endregion

		readonly AQISConcernType aQISConcernType;
	}
}
