using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.CAED.Send;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces.CAED;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.CAED
{
	public class CAEDSendMessageBuilder : MessageBuilderBase<Interchanges>
	{
		public CAEDSendMessageBuilder(ICAED header, ErrorCollector errorCollector, TransactionTypes transactionTypes)
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
			interchanges.Id = Placeholder.InterchangeId;
			interchanges.From = Placeholder.SenderPlatform;
			interchanges.To = MessageBuilderHelper.GetOptionalString(header.RecipientTiersProf);
			interchanges.MessageSet = GetMessageSets();
		}

		Collection<MessageSet> GetMessageSets()
		{
			return new Collection<MessageSet>
			{
				new MessageSet
				{
					Id = Placeholder.InterchangeId,
					Icid = Placeholder.InterchangeId,
					Date = ZDateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"),
					Emetteur = GetSender(),
					Destinataire = GetRecipient(),
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
					Id = Placeholder.InterchangeId,
					Type = DocumentType,
					Action = DocumentFunctionCode,
					ControlePrealable = GetDocument(),
				}
			};
		}

		ControlePrealableType GetDocument()
		{
			return new ControlePrealableType
			{
				ReferencesCtrl = GetReferences(),
				TiersCtrl = GetSiret(),
				LieuxCtrl = GetLocations(),
				LmarchandiseCtrl = GetGoodsItems(),
				EquipementCtrl = GetContainers(),
				DroitsCtrl = GetHarborDues(),
			};
		}

		ReferencesCtrlType GetReferences()
		{
			if (header.CommonAccessReference.IsEmpty)
			{
				errorCollector.AddError(Res.GetString("2D099671-BA77-45EB-8CAA-4DAA5DE4FC14", "Common Access Reference should not be empty."));
				return null;
			}
			else
			{
				return new ReferencesCtrlType
				{
					Rca = header.CommonAccessReference,
					Type = header.PortSystem == MGISystem ? MessageBuilderHelper.GetOptionalString(header.DeclarationType) : SOGETDeclarationType,
					Dos = MessageBuilderHelper.GetOptionalString(header.JobReference),
					Globale = MessageBuilderHelper.GetOptionalString(MessageBuilderHelper.GetYesOrNo(header.AppliesToAllPacks)),
				};
			}
		}

		TiersCtrlType GetSiret()
		{
			if (header.DeclarantsSIRETNumber.IsEmpty)
			{
				errorCollector.AddError(Res.GetString("8E18647A-293D-4C2F-816E-53FACD3CBF31", "Declarant SIRET Number should not be empty."));
				return null;
			}
			else
			{
				return new TiersCtrlType
				{
					Code = header.DeclarantsSIRETNumber,
				};
			}
		}

		LieuxCtrlType GetLocations()
		{
			return new LieuxCtrlType
			{
				Md = MessageBuilderHelper.GetOptionalString(header.CTOUser),
				Bdd = MessageBuilderHelper.GetOptionalString(header.CustomsDepartureOffice)
			};
		}

		LmarchandiseCtrlType GetGoodsItems()
		{
			if (header.TotalNumberOfPackages <= 0)
			{
				errorCollector.AddError(Res.GetString("64ED85D1-C0FC-4329-A2F1-7CC04A0FC1B9", "Total number of packages should be greater than 0."));
				return null;
			}
			else
			{
				return new LmarchandiseCtrlType
				{
					Nb = MessageBuilderHelper.GetOptionalPositiveInteger(header.TotalNumberOfPackages),
				};
			}
		}

		Collection<EquipementCtrlType> GetContainers()
		{
			var containers = header.Containers;
			if (containers != null && containers.Any(x => !x.IsEmpty))
			{
				return new Collection<EquipementCtrlType>(containers.Select(x => new EquipementCtrlType
				{
					Id = x,
				}).ToArray());
			}
			else
			{
				errorCollector.AddError(Res.GetString("5E14EA59-5627-41A3-B5A2-031432A9016F", "At least one container should be present."));
				return null;
			}
		}

		DroitsCtrl GetHarborDues()
		{
			if (header.Port.IsEmpty && header.HarborDuesAmount <= 0)
			{
				return null;
			}
			else
			{
				var droits = new DroitsCtrl
				{
					Port = MessageBuilderHelper.GetOptionalString(header.Port)
				};
				if (header.HarborDuesAmount > 0)
				{
					droits.Montant = Convert.ToUInt32(header.HarborDuesAmount);
					droits.Unite = MessageBuilderHelper.GetOptionalString(header.HarborDuesCurrency);
				}
				return droits;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Placeholder from EASYLOG")]
		static class Placeholder
		{
			internal const string InterchangeId = "#INTERCHANGE_ID#";
			internal const string SenderPlatform = "#EDI_FROM#";
		}

		const string DocumentType = "CAED";
		const string DocumentFunctionCode = "CREATE";
		const string MGISystem = "MGI";
		const string SOGETDeclarationType = "STI";

		readonly ICAED header;
		readonly ErrorCollector errorCollector;
		readonly TransactionTypes transactionTypes;
	}
}
