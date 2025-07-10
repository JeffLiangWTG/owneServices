using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DOA.Send;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces.DOA;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DOA
{
	public class DOASendMessageBuilder : MessageBuilderBase<Interchanges>
	{
		public DOASendMessageBuilder(IDOA header, ErrorCollector errorCollector, TransactionTypes transactionTypes)
		{
			this.header = header;
			this.errorCollector = errorCollector;
			this.transactionTypes = transactionTypes;
		}

		protected override Interchanges GenerateDeclarationMessage()
		{
			var message = new Interchanges();
			if (transactionTypes == TransactionTypes.Original)
			{
				PopulateInterchanges(message);
			}
			return message;
		}

		void PopulateInterchanges(Interchanges interchanges)
		{
			interchanges.Id = InterchangeIDPlaceholder;
			interchanges.From = SenderPlatformPlaceholder;
			interchanges.To = MessageBuilderHelper.GetOptionalString(header.RecipientTiersProf);
			interchanges.MessageSet = GetMessageSets();
		}

		Collection<MessageSet> GetMessageSets()
		{
			return new Collection<MessageSet>
			{
				new MessageSet
				{
					Id = InterchangeIDPlaceholder,
					Icid = InterchangeIDPlaceholder,
					Date = ZDateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"),
					Emetteur = GetSender(),
					Destinataire = Recipient,
					Messages = GetMessages()
				}
			};
		}

		Emetteur GetSender()
		{
			if (header.SenderUser.IsEmpty || header.SenderTiersProf.IsEmpty)
			{
				if (header.SenderUser.IsEmpty)
				{
					errorCollector.AddError(Res.GetString("B38A2BAE-E0A4-48A2-A655-EF2094A924D5", "User code of the message sender should not be empty."));
				}
				if (header.SenderTiersProf.IsEmpty)
				{
					errorCollector.AddError(Res.GetString("9C796C75-07EB-4168-992F-5BB9C89DD004", "Profession ID of the message sender should not be empty."));
				}
				return null;
			}
			else
			{
				return new Emetteur
				{
					User = MessageBuilderHelper.GetOptionalString(header.SenderUser),
					TiersProf = MessageBuilderHelper.GetOptionalString(header.SenderTiersProf)
				};
			}
		}

		Destinataire Recipient
		{
			get
			{
				if (recipient == null)
				{
					recipient = GetRecipient();
				}

				return recipient;
			}
		}
		Destinataire recipient;

		Destinataire GetRecipient()
		{
			if (header.RecipientUser.IsEmpty || header.RecipientTiersProf.IsEmpty)
			{
				if (header.RecipientUser.IsEmpty)
				{
					errorCollector.AddError(Res.GetString("582103AA-D1A7-4CF2-BE13-8C980716E364", "User code of the message recipient should not be empty."));
				}
				if (header.RecipientTiersProf.IsEmpty)
				{
					errorCollector.AddError(Res.GetString("AE5938A5-509B-4589-88FF-23D68C05B88F", "Profession ID of the message recipient should not be empty."));
				}
				return null;
			}
			else
			{
				return new Destinataire
				{
					User = MessageBuilderHelper.GetOptionalString(header.RecipientUser),
					TiersProf = MessageBuilderHelper.GetOptionalString(header.RecipientTiersProf)
				};
			}
		}

		Collection<RequestType> GetMessages()
		{
			return new Collection<RequestType>
			{
				new RequestType
				{
					Id = InterchangeIDPlaceholder,
					Type = DocumentType,
					Action = DocumentFunctionCode,
					DocumentAccompagnement = GetDocument()
				}
			};
		}

		DocumentAccompagnementType GetDocument()
		{
			return new DocumentAccompagnementType
			{
				ReferenceDoc = GetReferences(),
				ConteneurDoc = GetContainers(),
				TiersDoc = GetThirdParty(),
				LieuxDoc = GetLocations(),
				LmarchandiseDoc = GetGoodsItems(),
				DroitsDoc = GetHarborDues(),
				DescriptionDoc = GetTariffs()
			};
		}

		DocumentAccompagnementTypeReferenceDoc GetReferences()
		{
			if (header.DeclarationNumber.IsEmpty || header.DeclarationType.IsEmpty || header.JobReference.IsEmpty)
			{
				if (header.DeclarationNumber.IsEmpty)
				{
					errorCollector.AddError(Res.GetString("BFC4E55F-5C0A-4E43-B0A1-C4D4CC74151F", "Declaration number should not be empty."));
				}
				if (header.DeclarationType.IsEmpty)
				{
					errorCollector.AddError(Res.GetString("DE309886-3A4F-4E07-A755-B79B9794446D", "Declaration type should not be empty."));
				}
				if (header.JobReference.IsEmpty)
				{
					errorCollector.AddError(Res.GetString("3F654E19-B4D9-4B63-90D4-A24DB4E56DFF", "Job reference should not be empty."));
				}
				return null;
			}
			else
			{
				return new DocumentAccompagnementTypeReferenceDoc
				{
					Num = MessageBuilderHelper.GetOptionalString(header.DeclarationNumber),
					Type = header.PortSystem == MGISystem ? MessageBuilderHelper.GetOptionalString(header.DeclarationType) : SOGETDeclarationType,
					Rca = MessageBuilderHelper.GetOptionalString(header.CommonAccessReference),
					Eqd = MessageBuilderHelper.GetOptionalString(header.EquipmentReference),
					Ref = MessageBuilderHelper.GetOptionalString(header.DeclarationReference),
					Circuit = MessageBuilderHelper.GetOptionalString(header.DeclarationStatus),
					NumDoss = MessageBuilderHelper.GetOptionalString(header.JobReference),
					Anticip = MessageBuilderHelper.GetYesOrNo(header.Prelodged),
					Statut = DocumentStatus
				};
			}
		}

		Collection<ConteneurDocType> GetContainers()
		{
			if (header.Containers == null || header.Containers.Count <= 1)
			{
				return null;
			}

			return new Collection<ConteneurDocType>(header.Containers.Select(x => new ConteneurDocType
			{
				Eqd = x
			}).ToArray());
		}

		TiersDocType GetThirdParty()
		{
			return new TiersDocType
			{
				Sic = MessageBuilderHelper.GetOptionalString(header.SenderTiersProf)
			};
		}

		LieuxDocType GetLocations()
		{
			return new LieuxDocType
			{
				Bdd = header.CustomsDepartureOffice,
				Bds = header.CustomsDestinationOffice,
				Md = header.CTOUser
			};
		}

		LmarchandiseDocType GetGoodsItems()
		{
			if (header.TotalNumberOfPackages <= 0 || header.TotalGrossWeightInKilograms <= 0)
			{
				if (header.TotalNumberOfPackages <= 0)
				{
					errorCollector.AddError(Res.GetString("64ED85D1-C0FC-4329-A2F1-7CC04A0FC1B9", "Total number of packages should be greater than 0."));
				}
				if (header.TotalGrossWeightInKilograms <= 0)
				{
					errorCollector.AddError(Res.GetString("05DA09A7-D89E-402D-AD57-BA066DC05971", "Total gross weight should be greater than 0."));
				}
				return null;
			}
			else
			{
				return new LmarchandiseDocType
				{
					Nb = MessageBuilderHelper.GetOptionalPositiveInteger(header.TotalNumberOfPackages),
					Poids = MessageBuilderHelper.GetOptionalPositiveInteger(header.TotalGrossWeightInKilograms),
					Poidsnet = MessageBuilderHelper.GetOptionalPositiveInteger(header.TotalNetWeightInKilograms),
					Scelle = MessageBuilderHelper.GetYesOrNo(header.HasSeal)
				};
			}
		}

		DroitsDocType GetHarborDues()
		{
			if (header.HarborDuesAmount > 0)
			{
				return new DroitsDocType
				{
					Montant = Convert.ToUInt32(header.HarborDuesAmount),
					Unite = header.HarborDuesCurrency
				};
			}
			else
			{
				return null;
			}
		}

		Collection<DescriptionDocType> GetTariffs()
		{
			var tariffs = header.Tariffs;
			if (tariffs != null && tariffs.Any())
			{
				return new Collection<DescriptionDocType>(tariffs.Select(x => new DescriptionDocType
				{
					Ndp = x.Code,
					Description = x.Description
				}).ToArray());
			}
			else
			{
				return null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Placeholder from EASYLOG")]
		const string InterchangeIDPlaceholder = "#INTERCHANGE_ID#";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Placeholder from EASYLOG")]
		const string SenderPlatformPlaceholder = "#EDI_FROM#";
		const string DocumentType = "DOA";
		const string DocumentFunctionCode = "CREATE";
		const string DocumentStatus = "VA";
		const string MGISystem = "MGI";
		const string SOGETDeclarationType = "STI";

		readonly IDOA header;
		readonly ErrorCollector errorCollector;
		readonly TransactionTypes transactionTypes;
	}
}
