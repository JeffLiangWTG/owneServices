using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.CDS.CDSResponse
{
	public static class ResponseFunctionHelper
	{
		public static (ZBool Should, ZString NewStatus) ShouldUpdateCustomsStatus(this ResponseFunction responseFunction, BusinessObjectFactory factory, ZString originalStatus, ZString grouping)
		{
			switch (responseFunction)
			{
				case ResponseFunction.DeclarationAccepted _:
					return ((originalStatus.IsEmpty || originalStatus.EqualsIgnoringCase(Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue) || IsMessageRegistered(originalStatus)) && responseFunction.ShouldUpdateCustomsStatus(factory, grouping), responseFunction.ThreeCharFunctionCode);
				case ResponseFunction.MessageRegistered _:
					return ((originalStatus.IsEmpty || IsMessageRegistered(originalStatus)) && responseFunction.ShouldUpdateCustomsStatus(factory, grouping), responseFunction.ThreeCharFunctionCode);

				case ResponseFunction.DeclarationSubjectToPhysicalControl _:
				case ResponseFunction.DeclarationSubjectToPhysicalControl2 _:
					return ((originalStatus.IsEmpty || originalStatus.EqualsIgnoringCase(Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue) || IsDeclarationAccepted(originalStatus) || IsMessageRegistered(originalStatus)) && responseFunction.ShouldUpdateCustomsStatus(factory, grouping), responseFunction.ThreeCharFunctionCode);
				case ResponseFunction.DutiesTaxesCalculatedAndDue _:
					var statusToExclude = new ZString[] { Constants.ThreeCharFunctionCodes.DeclarationAccepted, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl, Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2 };
					if (!originalStatus.IsEmpty && (statusToExclude.Contains(originalStatus)))
					{
						return (false, originalStatus);
					}
					else
					{
						return ((originalStatus.IsEmpty || IsDeclarationAccepted(originalStatus) || IsMessageRegistered(originalStatus)) && responseFunction.ShouldUpdateCustomsStatus(factory, grouping), responseFunction.ThreeCharFunctionCode);
					}
				case ResponseFunction.GoodsMayBeReleased _:
				case ResponseFunction.DeclarationCleared _:
					return (responseFunction.ShouldUpdateCustomsStatus(factory, grouping), EntryStatusList.Codes.Clear);
				case ResponseFunction.DeclarationCancelled _:
					return (responseFunction.ShouldUpdateCustomsStatus(factory, grouping), EntryStatusList.Codes.Cancelled);
				case ResponseFunction.MessageRejected _:
					if (!originalStatus.IsEmpty && (originalStatus != EntryStatusList.Codes.Cancelled))
					{
						return (true, EntryStatusList.Codes.Cancelled);
					}
					else
					{
						return (false, ZString.Empty);
					}

				default:
					return (false, ZString.Empty);
			}
		}

		public static ZBool ShouldUpdateEntryNumber(this ResponseFunction responseFunction, BusinessObjectFactory factory, ZString grouping)
		{
			return CustomsStatusAttributeHelper.ShouldUpdateEntryNumber(factory, responseFunction.NumericFunctionCode, grouping, ZDateTime.Today);
		}

		static ZBool ShouldUpdateCustomsStatus(this ResponseFunction responseFunction, BusinessObjectFactory factory, ZString grouping)
		{
			return CustomsStatusAttributeHelper.ShouldUpdateCustomsStatus(factory, responseFunction.NumericFunctionCode, grouping, ZDateTime.Today);
		}

		public static ZBool ShouldSendEntryDocs(this ResponseFunction responseFunction, BusinessObjectFactory factory, ZString grouping)
		{
			return CustomsStatusAttributeHelper.ShouldSendEntryDocs(factory, responseFunction.NumericFunctionCode, grouping, ZDateTime.Today);
		}

		static ZBool IsDeclarationAccepted(ZString originalStatus)
		{
			return originalStatus == Constants.ThreeCharFunctionCodes.DeclarationAccepted;
		}

		static ZBool IsMessageRegistered(ZString originalStatus)
		{
			return originalStatus == Constants.ThreeCharFunctionCodes.MessageRegistered;
		}
	}
}
