using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISDocumentValidation : ZValidation
	{
		public AQISDocumentValidation(AQISDocument parent)
			: base(parent)
		{
			this.aQISDocument = parent;
		}

		public override void ValidateAll()
		{
			ValidateType();
			ValidateNumber();
		}

		public override Type AutoValidationType
		{
			get { return typeof(AQISDocumentValidation); }
		}

		#region Number

		public void ValidateNumber()
		{
			ValidateCalculatedProperty(aQISDocument.NumberInfo);
		}

		protected void CheckNumber()
		{
			if (!aQISDocument.IsValidationSuspended)
			{
				if (!aQISDocument.Type.IsEmpty && aQISDocument.Number.IsEmpty)
				{
					aQISDocument.NumberInfo.AddError("Please enter a Document Number");
				}

				if (aQISDocument.Number.Contains(','))
				{
					aQISDocument.NumberInfo.AddError("You cannot use the character ',' in a Document Number");
				}

				ValidateOnly10Records();
			}
		}

		#endregion

		#region Type

		public void ValidateType()
		{
			ValidateCalculatedProperty(aQISDocument.TypeInfo);
		}

		protected void CheckType()
		{
			if (!aQISDocument.IsValidationSuspended)
			{
				if (aQISDocument.Type.IsEmpty && !aQISDocument.Number.IsEmpty)
				{
					aQISDocument.TypeInfo.AddError("Please enter a Document Type");
				}

				if (!aQISDocument.Type.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(aQISDocument.TypeInfo, aQISDocument.Lookups.AQISDocumentTypeList);
				}

				if (aQISDocument.Type.Contains('/') || aQISDocument.Type.Contains(','))
				{
					aQISDocument.TypeInfo.AddError("You cannot use the characters '/' or ','");
				}

				ValidateOnly10Records();
			}
		}

		#endregion

		void ValidateOnly10Records()
		{
			if (aQISDocument.ParentCollections.Count > 0)
			{
				aQISDocument.ClearRowNotifications();

				AQISDocumentCollection documents = ((AQISDocumentCollection)aQISDocument.ParentCollections.First());

				if (documents != null && documents.Count > 10 && !documents[10].Type.IsEmpty && !documents[10].Number.IsEmpty)
				{
					aQISDocument.AddRowError("You can only enter 10 AQIS Document Types and Numbers");
				}
			}
		}

		readonly AQISDocument aQISDocument;
	}
}
