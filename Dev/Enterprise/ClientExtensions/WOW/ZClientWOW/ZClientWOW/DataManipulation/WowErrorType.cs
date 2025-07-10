using System;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.Wow
{
	[Serializable]
	internal class WowErrorType : ErrorType
	{
		internal WowErrorType(string message) : base(message)
		{
		}

		public static readonly ErrorType PartTakenOnByMoreThan1Buyer = new WowErrorType("Part has been assigned to more than 1 buyer on multiple orders");
		public static readonly ErrorType MoreThan1PartNumber = new WowErrorType("More than 1 suitable part found for order line");
		public static readonly ErrorType EdiTrackEmailNotSet = new WowErrorType("EdiTrack email address not set up. You must set this value in the system registry for this process to function.");
		public static readonly ErrorType CustomsDeclarationMergeError = new WowErrorType("Merge error");
	}
}
