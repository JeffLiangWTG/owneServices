using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CDS.CDSResponse
{
	public abstract class ResponseFunction
	{
		public abstract ZString NumericFunctionCode { get; }
		public abstract ZString ThreeCharFunctionCode { get; }
		public abstract ZString Description { get; }
		public abstract ZString OldCHIEFReportCode { get; }
		public abstract ZString Category { get; }

		public virtual ZString GetDescription(CusEntryHeader entryHeader) => Description;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ResponseFunction New(ZString numericFunctionCode)
		{
			switch (numericFunctionCode)
			{
				case Constants.NumbericFunctionCodes.DeclarationAccepted:
					return new DeclarationAccepted();
				case Constants.NumbericFunctionCodes.MessageRegistered:
					return new MessageRegistered();
				case Constants.NumbericFunctionCodes.MessageRejected:
					return new MessageRejected();
				case Constants.NumbericFunctionCodes.DeclarationIncomplete:
					return new DeclarationIncomplete();
				case Constants.NumbericFunctionCodes.DeclarationSubjectToPhysicalControl:
					return new DeclarationSubjectToPhysicalControl();
				case Constants.NumbericFunctionCodes.DeclarationSubjectToPhysicalControl2:
					return new DeclarationSubjectToPhysicalControl2();
				case Constants.NumbericFunctionCodes.DeclarationUpdatedByCustoms:
					return new DeclarationUpdatedByCustoms();
				case Constants.NumbericFunctionCodes.GoodsMayBeReleased:
					return new GoodsMayBeReleased();
				case Constants.NumbericFunctionCodes.DeclarationCleared:
					return new DeclarationCleared();
				case Constants.NumbericFunctionCodes.DeclarationCancelled:
					return new DeclarationCancelled();
				case Constants.NumbericFunctionCodes.AdditionalMessageProcessed:
					return new AdditionalMessageProcessed();
				case Constants.NumbericFunctionCodes.DutiesTaxesCalculatedAndDue:
					return new DutiesTaxesCalculatedAndDue();
				case Constants.NumbericFunctionCodes.InsufficientDefermentBalance:
					return new InsufficientDefermentBalance();
				case Constants.NumbericFunctionCodes.PaymentDue:
					return new PaymentDue();
				case Constants.NumbericFunctionCodes.GoodsExitedCustomsUnion:
					return new GoodsExitedCustomsUnion();
				case Constants.NumbericFunctionCodes.ExceptionalIrregularityNeedsToBeHandled:
					return new ExceptionalIrregularityNeedsToBeHandled();
				case Constants.NumbericFunctionCodes.ExitOfGoodsFromEUNotConfirmed:
					return new ExitOfGoodsFromEUNotConfirmed();
				case Constants.NumbericFunctionCodes.DefraControl:
					return new DefraControl();
				case Constants.NumbericFunctionCodes.IncomingQueryNotification:
					return new IncomingQueryNotification();
				default:
					return new Empty();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZString GetDescription(ZString threeCharFunctionCode)
		{
			switch (threeCharFunctionCode)
			{
				case Constants.ThreeCharFunctionCodes.DeclarationAccepted:
					return new DeclarationAccepted().Description;
				case Constants.ThreeCharFunctionCodes.MessageRegistered:
					return new MessageRegistered().Description;
				case Constants.ThreeCharFunctionCodes.MessageRejected:
					return new MessageRejected().Description;
				case Constants.ThreeCharFunctionCodes.DeclarationIncomplete:
					return new DeclarationIncomplete().Description;
				case Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl:
					return new DeclarationSubjectToPhysicalControl().Description;
				case Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2:
					return new DeclarationSubjectToPhysicalControl2().Description;
				case Constants.ThreeCharFunctionCodes.DeclarationUpdatedByCustoms:
					return new DeclarationUpdatedByCustoms().Description;
				case Constants.ThreeCharFunctionCodes.GoodsMayBeReleased:
					return new GoodsMayBeReleased().Description;
				case Constants.ThreeCharFunctionCodes.DeclarationCleared:
					return new DeclarationCleared().Description;
				case Constants.ThreeCharFunctionCodes.DeclarationCancelled:
					return new DeclarationCancelled().Description;
				case Constants.ThreeCharFunctionCodes.AdditionalMessageProcessed:
					return new AdditionalMessageProcessed().Description;
				case Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue:
					return new DutiesTaxesCalculatedAndDue().Description;
				case Constants.ThreeCharFunctionCodes.InsufficientDefermentBalance:
					return new InsufficientDefermentBalance().Description;
				case Constants.ThreeCharFunctionCodes.PaymentDue:
					return new PaymentDue().Description;
				case Constants.ThreeCharFunctionCodes.GoodsExitedCustomsUnion:
					return new GoodsExitedCustomsUnion().Description;
				case Constants.ThreeCharFunctionCodes.ExceptionalIrregularityNeedsToBeHandled:
					return new ExceptionalIrregularityNeedsToBeHandled().Description;
				case Constants.ThreeCharFunctionCodes.ExitOfGoodsFromEUNotConfirmed:
					return new ExitOfGoodsFromEUNotConfirmed().Description;
				case Constants.ThreeCharFunctionCodes.DefraControl:
					return new DefraControl().Description;
				case Constants.ThreeCharFunctionCodes.IncomingQueryNotification:
					return new IncomingQueryNotification().Description;
				default:
					return new Empty().Description;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZString GetNumericFunctionCode(ZString threeCharFunctionCode)
		{
			switch (threeCharFunctionCode)
			{
				case Constants.ThreeCharFunctionCodes.DeclarationAccepted:
					return new DeclarationAccepted().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.MessageRegistered:
					return new MessageRegistered().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.MessageRejected:
					return new MessageRejected().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.DeclarationIncomplete:
					return new DeclarationIncomplete().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl:
					return new DeclarationSubjectToPhysicalControl().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2:
					return new DeclarationSubjectToPhysicalControl2().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.DeclarationUpdatedByCustoms:
					return new DeclarationUpdatedByCustoms().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.GoodsMayBeReleased:
					return new GoodsMayBeReleased().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.DeclarationCleared:
				case EntryStatusList.Codes.Clear:
					return new DeclarationCleared().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.DeclarationCancelled:
				case EntryStatusList.Codes.Cancelled:
					return new DeclarationCancelled().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.AdditionalMessageProcessed:
					return new AdditionalMessageProcessed().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue:
					return new DutiesTaxesCalculatedAndDue().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.InsufficientDefermentBalance:
					return new InsufficientDefermentBalance().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.PaymentDue:
					return new PaymentDue().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.GoodsExitedCustomsUnion:
					return new GoodsExitedCustomsUnion().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.ExceptionalIrregularityNeedsToBeHandled:
					return new ExceptionalIrregularityNeedsToBeHandled().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.ExitOfGoodsFromEUNotConfirmed:
					return new ExitOfGoodsFromEUNotConfirmed().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.DefraControl:
					return new DefraControl().NumericFunctionCode;
				case Constants.ThreeCharFunctionCodes.IncomingQueryNotification:
					return new IncomingQueryNotification().NumericFunctionCode;
				default:
					return new Empty().NumericFunctionCode;
			}
		}

		public class Empty : ResponseFunction
		{
			public override ZString NumericFunctionCode => ZString.Empty;

			public override ZString ThreeCharFunctionCode => ZString.Empty;

			public override ZString Description => ZString.Empty;

			public override ZString OldCHIEFReportCode => ZString.Empty;

			public override ZString Category => ZString.Empty;
		}

		public class DeclarationAccepted : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.DeclarationAccepted;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.DeclarationAccepted;

			public override ZString Description => "Declaration has been legally accepted";

			public override ZString OldCHIEFReportCode => Constants.OldCHIEFReportCodes.DeclarationAccepted;

			public override ZString Category => Constants.ResponseFunctionCategory.PositiveReplies;
		}

		public class MessageRegistered : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.MessageRegistered;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.MessageRegistered;

			public override ZString Description => "Message has been registered";

			public override ZString OldCHIEFReportCode => Constants.OldCHIEFReportCodes.MessageRegistered;

			public override ZString Category => Constants.ResponseFunctionCategory.PositiveReplies;
		}

		public class MessageRejected : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.MessageRejected;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.MessageRejected;

			public override ZString Description => "Message has been rejected";

			public override ZString GetDescription(CusEntryHeader entryHeader)
			{
				return (string)entryHeader?.CH_EntryStatus == EDIMessageStatusList.Codes.Cancelled ? "Pre-lodged declaration canceled OK" : Description;
			}

			public override ZString OldCHIEFReportCode => Constants.OldCHIEFReportCodes.MessageRejected;

			public override ZString Category => Constants.ResponseFunctionCategory.Rejections;
		}

		public class DeclarationIncomplete : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.DeclarationIncomplete;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.DeclarationIncomplete;

			public override ZString Description => "Declaration is incomplete";

			public override ZString OldCHIEFReportCode => ZString.Empty;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class DeclarationSubjectToPhysicalControl : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.DeclarationSubjectToPhysicalControl;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl;

			public override ZString Description => "Declaration is subject to physical control";

			public override ZString OldCHIEFReportCode => Constants.OldCHIEFReportCodes.DeclarationSubjectToPhysicalControl;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class DeclarationSubjectToPhysicalControl2 : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.DeclarationSubjectToPhysicalControl2;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2;

			public override ZString Description => "Declaration is subject to physical control";

			public override ZString OldCHIEFReportCode => ZString.Empty;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class DeclarationUpdatedByCustoms : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.DeclarationUpdatedByCustoms;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.DeclarationUpdatedByCustoms;

			public override ZString Description => "Declaration has been updated by Customs";

			public override ZString OldCHIEFReportCode => ZString.Empty;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class GoodsMayBeReleased : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.GoodsMayBeReleased;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.GoodsMayBeReleased;

			public override ZString Description => "Goods may now be released";

			public override ZString OldCHIEFReportCode => Constants.OldCHIEFReportCodes.GoodsMayBeReleased;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class DeclarationCleared : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.DeclarationCleared;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.DeclarationCleared;

			public override ZString Description => "Declaration is now cleared";

			public override ZString OldCHIEFReportCode => ZString.Empty;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class DeclarationCancelled : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.DeclarationCancelled;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.DeclarationCancelled;

			public override ZString Description => "Declaration has been canceled";

			public override ZString OldCHIEFReportCode => Constants.OldCHIEFReportCodes.DeclarationCancelled;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class AdditionalMessageProcessed : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.AdditionalMessageProcessed;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.AdditionalMessageProcessed;

			public override ZString Description => "Additional message has been processed";

			public override ZString OldCHIEFReportCode => ZString.Empty;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class DutiesTaxesCalculatedAndDue : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.DutiesTaxesCalculatedAndDue;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue;

			public override ZString Description => "Duties and taxes have been calculated and are due";

			public override ZString OldCHIEFReportCode => Constants.OldCHIEFReportCodes.DutiesTaxesCalculatedAndDue;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class InsufficientDefermentBalance : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.InsufficientDefermentBalance;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.InsufficientDefermentBalance;

			public override ZString Description => "Insufficient deferment balance";

			public override ZString OldCHIEFReportCode => Constants.OldCHIEFReportCodes.InsufficientDefermentBalance;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class PaymentDue : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.PaymentDue;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.PaymentDue;

			public override ZString Description => "Payment is due (reminder)";

			public override ZString OldCHIEFReportCode => ZString.Empty;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class GoodsExitedCustomsUnion : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.GoodsExitedCustomsUnion;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.GoodsExitedCustomsUnion;

			public override ZString Description => "Goods have exited the Customs Union";

			public override ZString OldCHIEFReportCode => ZString.Empty;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class ExceptionalIrregularityNeedsToBeHandled : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.ExceptionalIrregularityNeedsToBeHandled;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.ExceptionalIrregularityNeedsToBeHandled;

			public override ZString Description => "Exceptional irregularity needs to be handled";

			public override ZString OldCHIEFReportCode => ZString.Empty;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class ExitOfGoodsFromEUNotConfirmed : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.ExitOfGoodsFromEUNotConfirmed;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.ExitOfGoodsFromEUNotConfirmed;

			public override ZString Description => "Exit of goods from Customs Union is not yet confirmed";

			public override ZString OldCHIEFReportCode => Constants.OldCHIEFReportCodes.ExitOfGoodsFromEUNotConfirmed;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class DefraControl : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.DefraControl;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.DefraControl;

			public override ZString Description => "DEFRA control applied";

			public override ZString OldCHIEFReportCode => ZString.Empty;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}

		public class IncomingQueryNotification : ResponseFunction
		{
			public override ZString NumericFunctionCode => Constants.NumbericFunctionCodes.IncomingQueryNotification;

			public override ZString ThreeCharFunctionCode => Constants.ThreeCharFunctionCodes.IncomingQueryNotification;

			public override ZString Description => "Incoming query notification";

			public override ZString OldCHIEFReportCode => Constants.OldCHIEFReportCodes.IncomingQueryNotification;

			public override ZString Category => Constants.ResponseFunctionCategory.UnsolicitedUpdates;
		}
	}
}
