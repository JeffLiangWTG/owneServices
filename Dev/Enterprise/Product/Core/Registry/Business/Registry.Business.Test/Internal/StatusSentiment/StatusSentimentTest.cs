using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StatusSentiment))]
	sealed class StatusSentimentTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestStatusCode()
		{
			var statusSentiment = GetBusinessObjectToSerialise() as StatusSentiment;
			AssertEquals("BKD", statusSentiment.Code);
			AssertNoErrors(statusSentiment.CodeInfo);

			statusSentiment.Code = string.Empty;
			AssertHasError(statusSentiment.CodeInfo, "Please enter a Code.");

			AssertExceptionThrown("StatusCode has max length of three", typeof(MaxLengthExceededException), () => statusSentiment.Code = "ABCD");
			ErrorReporter.Clear();

			statusSentiment.Code = "SUC";
			var sentimentCollection = new StatusSentimentCollection
			{
				statusSentiment
			};
			var duplicateSentiment = GetBusinessObjectToSerialise() as StatusSentiment;
			sentimentCollection.Add(duplicateSentiment);
			duplicateSentiment.Code = "SUC";
			AssertHasError(duplicateSentiment.CodeInfo, "The Code has been duplicated and must be unique.");
		}

		public void TestStatusDescription()
		{
			var statusSentiment = GetBusinessObjectToSerialise() as StatusSentiment;
			AssertEquals("Booked", statusSentiment.Description);
			AssertNoErrors(statusSentiment.DescriptionInfo);

			statusSentiment.Description = string.Empty;
			AssertHasError(statusSentiment.DescriptionInfo, "Please enter a Description.");
		}

		public void TestStatusSentiment()
		{
			var statusSentiment = GetBusinessObjectToSerialise() as StatusSentiment;
			AssertEquals(StatusSentiment.SentimentTypes.Success, statusSentiment.Sentiment);
			AssertNoErrors(statusSentiment.SentimentInfo);

			statusSentiment.Sentiment = StatusSentiment.SentimentTypes.Info;
			AssertNoErrors(statusSentiment.SentimentInfo);

			statusSentiment.Sentiment = StatusSentiment.SentimentTypes.Primary;
			AssertNoErrors(statusSentiment.SentimentInfo);

			statusSentiment.Sentiment = StatusSentiment.SentimentTypes.Warning;
			AssertNoErrors(statusSentiment.SentimentInfo);

			statusSentiment.Sentiment = StatusSentiment.SentimentTypes.Critical;
			AssertNoErrors(statusSentiment.SentimentInfo);

			statusSentiment.Sentiment = string.Empty;
			AssertNoErrors(statusSentiment.SentimentInfo);

			statusSentiment.Sentiment = "Invalid Sentiment";
			AssertHasError(statusSentiment.SentimentInfo, "Enter a valid Sentiment.");
		}

		public void TestStatusVariant()
		{
			var statusSentiment = GetBusinessObjectToSerialise() as StatusSentiment;
			AssertEquals(StatusSentiment.VariantTypes.Outline, statusSentiment.Variant);
			AssertNoErrors(statusSentiment.VariantInfo);

			statusSentiment.Variant = StatusSentiment.VariantTypes.Fill;
			AssertNoErrors(statusSentiment.VariantInfo);

			statusSentiment.Variant = string.Empty;
			AssertNoErrors(statusSentiment.VariantInfo);

			statusSentiment.Variant = "Invalid Variant";
			AssertHasError(statusSentiment.VariantInfo, "Enter a valid Variant.");
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var statusSentiment = new StatusSentiment(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			statusSentiment.Code = "BKD";
			statusSentiment.Description = "Booked";
			statusSentiment.Sentiment = StatusSentiment.SentimentTypes.Success;
			statusSentiment.Variant = StatusSentiment.VariantTypes.Outline;
			return statusSentiment;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
