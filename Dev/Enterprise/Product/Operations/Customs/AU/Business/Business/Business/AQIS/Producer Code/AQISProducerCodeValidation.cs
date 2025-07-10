using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISProducerCodeValidation : AQISSingleValueValidation
	{
		public AQISProducerCodeValidation(AQISProducerCode parent)
			: base(parent)
		{
			this.aQISProducerCode = parent;
		}

		public override ZInt MaximumNumberOfRecordsAllowed
		{
			get { return 10; }
		}

		public override void ValidateCodeAgainstLookupList()
		{
			ListValidation.MessageErrorIfInvalidCode(aQISProducerCode.CodeInfo, aQISProducerCode.Lookups.AQISProducerCodeList);
		}

		protected override void CheckCode()
		{
			base.CheckCode();

			if (!aQISProducerCode.IsValidationSuspended)
			{
				if (aQISProducerCode.ParentCollections.Count > 0)
				{
					AQISProducerCodeCollection aQISProducerCodes = ((AQISProducerCodeCollection)aQISProducerCode.ParentCollections.First());

					foreach (AQISProducerCode currentProducerCode in aQISProducerCodes)
					{
						if (currentProducerCode.Code != "" && currentProducerCode != aQISProducerCode && currentProducerCode.Code == aQISProducerCode.Code)
						{
							aQISProducerCode.CodeInfo.AddMessageError("You cannot repeat Producer Codes. Please select different Producer Codes.");
							break;
						}
					}
				}
			}
		}

		readonly AQISProducerCode aQISProducerCode;
	}
}
