using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class AQISSingleValueValidation : ZValidation
	{
		public AQISSingleValueValidation(AQISSingleValueBusinessObject parent)
			: base(parent)
		{
			this.aQISSingleValueBusinessObject = parent;
		}

		public override void ValidateAll()
		{
			ValidateCode();
		}

		public override Type AutoValidationType
		{
			get { return typeof(AQISSingleValueBusinessObject); }
		}

		#region Code

		public void ValidateCode()
		{
			ValidateCalculatedProperty(aQISSingleValueBusinessObject.CodeInfo);
		}

		protected virtual void CheckCode()
		{
			if (!aQISSingleValueBusinessObject.IsValidationSuspended)
			{
				ValidateCodeAgainstLookupList();

				if (aQISSingleValueBusinessObject.ParentCollections.Count > 0)
				{
					AQISSingleValueCollection elements = ((AQISSingleValueCollection)aQISSingleValueBusinessObject.ParentCollections.First());

					if (elements != null && elements.Count > MaximumNumberOfRecordsAllowed && !elements[MaximumNumberOfRecordsAllowed].Code.IsEmpty)
					{
						aQISSingleValueBusinessObject.CodeInfo.AddError("You can only enter " + MaximumNumberOfRecordsAllowed + " values");
					}
				}

				ZStringBuilder prohibitedCharsUsed = new ZStringBuilder();
				foreach (char currentChar in CharsNotAllowed)
				{
					if (aQISSingleValueBusinessObject.Code.Contains(currentChar))
					{
						prohibitedCharsUsed.Append("'");
						prohibitedCharsUsed.Append(currentChar.ToString());
						prohibitedCharsUsed.Append("'");
						prohibitedCharsUsed.Append(" ");
					}
				}

				if (!prohibitedCharsUsed.IsEmpty)
				{
					aQISSingleValueBusinessObject.CodeInfo.AddError("You cannot use the characters " + prohibitedCharsUsed.ToString().TrimEnd(' '));
				}
			}
		}

		public abstract void ValidateCodeAgainstLookupList();
		public abstract ZInt MaximumNumberOfRecordsAllowed { get; }

		protected internal virtual char[] CharsNotAllowed
		{
			get { return new char[] { ',' }; }
		}

		#endregion

		readonly AQISSingleValueBusinessObject aQISSingleValueBusinessObject;
	}
}
