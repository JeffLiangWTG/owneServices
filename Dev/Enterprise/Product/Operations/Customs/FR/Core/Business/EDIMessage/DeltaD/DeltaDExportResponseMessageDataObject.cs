using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Response.Export;
using CargoWise.Types;
using TMessage = CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Response.Export.TMessage;
using TTaxationDetail = CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Response.Export.TTaxationDetail;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DeltaDExportResponseMessageDataObject : MessageDataObject<TMessage>, IResponseDeltaDExportDataProvider
	{
		public DeltaDExportResponseMessageDataObject(FREDIMessage message)
			: base(message)
		{
		}
		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new DeltaDExportResponsePrettier(this);
		}
		public ZBool IsImport => false;
		public ZString TransactionID => ResponseMessage?.EnveloppeMessage.TransactionId;
		public ZString Refdec => ReponseDeclaration?.Entete?.Refdec;
		public ZString Etat => notificationEtat?.Etat;
		public ZString EtatECS => notificationEtat?.EtatEcs;
		public ZString EtatDate => notificationEtat?.EtatDate.Trim();
		public ZString EtatHeure => notificationEtat?.EtatHeure.Trim();
		public ZString Refdos => ReponseDeclaration?.Entete?.Refdos;
		public ZBool HasLiquidation => Notification != null && Notification.ReponseArticles.Any();
		public Collection<TLiquidationArt> Liquidation => Notification?.ReponseArticles ?? new Collection<TLiquidationArt>();
		public ZBool IsGvmsAdvice => false;
		public ZString Evenement => ZString.Empty;
		public ZLong? Montantcautionnable => Notification?.LiquidationGen?.Montantcautionnable;
		public ZBool HasErrors
		{
			get
			{
				var erreurGen = ReponseDeclaration?.ReponseDatas?.Erreur?.ErreurGen;
				return erreurGen != null && erreurGen.Any();
			}
		}

		public ZString mrn => ReponseDeclaration?.Entete?.Mrn;

		public IEnumerable<IResponseLiquidationWrapper> ResponseLiquidations
		{
			get
			{
				foreach (var responseArticle in Liquidation)
				{
					yield return new DeltaDExportResponseLiquidationWrapper(responseArticle);
				}
			}
		}

		public ZBool HasArticleDAU => ResponseArticleDAUs.Any();

		public IEnumerable<IResponseArticleDAUWrapper> ResponseArticleDAUs
		{
			get
			{
				if (responseArticleDAUs == null)
				{
					responseArticleDAUs = new List<IResponseArticleDAUWrapper>();
					if (Notification?.DatasExport?.ArticleDaUs != null)
					{
						foreach (var articleDAU in Notification.DatasExport.ArticleDaUs)
						{
							responseArticleDAUs.Add(new DeltaDExportResponseArticleDAUWrapper(articleDAU));
						}
					}
				}
				return responseArticleDAUs;
			}
		}

		List<IResponseArticleDAUWrapper> responseArticleDAUs;

		class DeltaDExportResponseLiquidationWrapper : IResponseLiquidationWrapper
		{
			public DeltaDExportResponseLiquidationWrapper(TLiquidationArt responseArticle)
			{
				this.responseArticle = Argument.NotNull(responseArticle, nameof(responseArticle));
			}
			readonly TLiquidationArt responseArticle;
			public ZShort numart => responseArticle.Numart;
			public IEnumerable<ITaxDetailWrapper> TaxDetails
			{
				get
				{
					foreach (var taxDetail in responseArticle.TTaxationDetails)
					{
						yield return new DeltaDExportTaxDetailWrapper(taxDetail);
					}
				}
			}
		}

		class DeltaDExportResponseArticleDAUWrapper : IResponseArticleDAUWrapper
		{
			public DeltaDExportResponseArticleDAUWrapper(TdauExportArticleDaUsArticleDau articleDau)
			{
				this.articleDau = Argument.NotNull(articleDau, nameof(articleDau));
			}
			readonly TdauExportArticleDaUsArticleDau articleDau;

			public ZShort numart => articleDau.Article?.Numart ?? 0;
			public ZDecimal? Valstat => ZDecimal.Zero;
			public ZDecimal? Valdou => articleDau?.Article?.Facturation?.Valdou;
			public ZDecimal? Asstva => ZDecimal.Zero;
		}

		class DeltaDExportTaxDetailWrapper : ITaxDetailWrapper
		{
			public DeltaDExportTaxDetailWrapper(TTaxationDetail taxDetail)
			{
				this.taxDetail = Argument.NotNull(taxDetail, nameof(taxDetail));
			}
			readonly TTaxationDetail taxDetail;
			public ZString typtax => taxDetail.Typtax;
			public ZString codtax => taxDetail.Codtax;
			public ZDecimal asstax => taxDetail.Asstax ?? decimal.Zero;
			public ZDecimal quotax => taxDetail.Quotax ?? decimal.Zero;
			public ZLong montanttax => taxDetail.Montanttax;
			public ZString statutLiquidation => taxDetail.Statutliquidation;
			public ZString SuppUnitsMethodOfCalculation => LiquidationItemHasSuppUnits ? taxDetail.UniSpe.Unispe + taxDetail.UniSpe.Qualifunispe : string.Empty;
			public ZBool LiquidationItemHasSuppUnits => !string.IsNullOrEmpty(taxDetail.UniSpe?.Unispe);
		}

		ReponseDeclaration ReponseDeclaration => ResponseMessage?.ReponseDeclaration;
		TNotification Notification => ReponseDeclaration?.ReponseDatas?.Notification;
		TEtat notificationEtat => Notification?.Etat;
	}
}
