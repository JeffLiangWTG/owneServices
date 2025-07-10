using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCInboundInterchangeProcessor : InboundInterchangeProcessor, ICustomsServiceTaskProcess
	{
		public AUCInboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string[] ApplicationCodes => new string[]
		{
			EDIInterchange.ApplicationCodes.CMR,
			EDIInterchange.ApplicationCodes.EXDOC,
			EDIInterchange.ApplicationCodes.OneStop
		};

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return messageCreator ?? (messageCreator = new InboundMessageCreator(Logger, IsInterchangeNotDeleted));
		}
		InboundMessageCreator messageCreator;

		public override bool IsInterchangeNotDeleted(EDIInterchange interchange)
		{
			return !interchange.IsDeleted && interchange.EI_Status != EDIInterchange.Status.Error;
		}

		public override void Dispose()
		{
			messageCreator?.Dispose();
		}

		#region InboundMessageCreator Class

		class InboundMessageCreator : IInboundMessageCreator, IDisposable
		{
			public InboundMessageCreator(LoggingInformation logger, IsInterchangeNotDeletedDelegate del)
			{
				this.logger = logger;
				this.isInterchangeNotDeleted = del;
			}
			readonly LoggingInformation logger;
			readonly IsInterchangeNotDeletedDelegate isInterchangeNotDeleted;

			public delegate bool IsInterchangeNotDeletedDelegate(EDIInterchange interchange);

			void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
			{
				logger.Log(string.Format("Processing interchange #{0}, Application Code '{1}'", interchange.EI_InterchangeNum, interchange.EI_ApplicationCode));
				if (interchange.EI_ApplicationCode == EDIInterchange.ApplicationCodes.CMR && interchange.EI_HeaderText.IsEmpty &&
								interchange.EI_BodyText.Left(400).ToLower().Contains("smime-type=signed-data"))
				{
					ProcessSignedCMRInterchange(interchange);
				}
				if (isInterchangeNotDeleted(interchange))
				{
					if (interchange.EI_InterchangeType.IsEmpty)
					{
						interchange.EI_InterchangeType = interchange.EI_ApplicationCode;
					}

					interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived(false);
					logger.DebugLog(string.Format("Messages spawned from interchange #{0}", interchange.EI_InterchangeNum));
				}
			}

			void ProcessSignedCMRInterchange(EDIInterchange interchange)
			{
				var interchangeText = GetVerifiedTextAndSaveSigningCertificate(interchange.EI_BodyText);
				logger.DebugLog(string.Format("Signature verified for interchange #{0}", interchange.EI_InterchangeNum));

				if (!AUBatchProcessorSupporter.IsATextMessage(interchangeText, logger, null))
				{
					ZString from = interchange.EI_From;
					ZString interchangeNum = interchange.EI_InterchangeNum;
					var characterSet = EDIInterchange.GetCharacterSetFromInterchangeString(interchangeText.ToString());
					var interchangeDetails = EDIInterchange.GetInterchangeDetailsFromString(interchange.Factory, interchangeText, EDIInterchange.ApplicationCodes.CMR, false, false, characterSet);
					interchange.EI_HeaderText = interchangeDetails.HeaderText;
					interchange.EI_BodyText = interchangeDetails.BodyText;
					interchange.EI_FooterText = interchangeDetails.FooterText;
					interchange.EI_From = interchangeDetails.From;
					interchange.EI_InterchangeNum = interchangeDetails.InterchangeNum;

					if (interchange.EI_NeedsAcknowledgement)
					{
						AUBatchProcessorSupporter.CreateCMRAcknowledgementMessage(interchange);
						logger.DebugLog(string.Format("Acknowledgement message generated for inbound interchange #{0}", interchange.EI_InterchangeNum));
					}

					if (interchange.ExistingInterchangeMatchingToFromAndInterchangeNum != null)
					{
						logger.LogWarning(string.Format("CMR inbound interchange #{0} acknowledged, but ignored, as it is a duplicate", interchange.EI_InterchangeNum));
						interchange.EI_Status = EDIInterchange.Status.Error;
						interchange.EI_From = from;
						interchange.EI_InterchangeNum = interchangeNum;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						interchange.Logs.AddNew(Events.EditedARecord, "DUPLICATE");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
					else
					{
						logger.DebugLog(string.Format("CMR interchange #{0} added to DB", interchange.EI_InterchangeNum));
					}
				}
			}

			ZString GetVerifiedTextAndSaveSigningCertificate(ZString signedContent)
			{
				string result = signedContent.Substring(signedContent.IndexOf("\r\n\r\n") + 4);
				var certManager = CertificateManager;
				var trustPointCertificate = certManager.TrustPointCertificate;
				var signersName = certManager.EncryptionCertificateName;

				if (trustPointCertificate != null && signersName != null)
				{
					try
					{
						var verifiedMessage = trustPointCertificate.VerifyBytes(result, signersName);
						result = Enterprise.Customs.Business.BatchProcessor.BatchProcessorSupporter.DecodeMIMEText(verifiedMessage.Content);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var message = $@"Verifying signature of Customs MQ message failed, or there was a problem loading the certificate.
Error: {ex.Message}
Signer: '{signersName}'
Message Body: {signedContent}";
						logger.LogError(message);
						throw new System.Security.Cryptography.CryptographicException(message, ex);
					}
				}
				return result;
			}

			CertificateManager CertificateManager => certificateManager ?? (certificateManager = new CertificateManager(new BusinessObjectFactory()));
			CertificateManager certificateManager;

			public void Dispose()
			{
				certificateManager?.Dispose();
			}
		}

		#endregion
	}
}
