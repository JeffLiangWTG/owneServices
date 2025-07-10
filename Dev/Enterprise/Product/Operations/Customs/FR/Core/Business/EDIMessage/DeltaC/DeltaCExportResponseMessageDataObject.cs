using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Response.Export;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DeltaCExportResponseMessageDataObject : MessageDataObject<TMessage>, IResponseDeltaCExportDataProvider
	{
		public DeltaCExportResponseMessageDataObject(FREDIMessage message)
			: base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new DeltaCExportResponsePrettier(this);
		}

		public ZBool IsImport => false;
		public ZString TransactionID => ResponseMessage?.EnveloppeMessage.TransactionId;
		public ZString Refdec => ReponseDeclaration?.Entete?.Refdec;
		public ZString Etat => notificationEtat?.Etat;
		public ZString EtatECS => notificationEtat?.EtatECS;
		public ZString EtatDate => notificationEtat?.EtatDate.Trim();
		public ZString EtatHeure => notificationEtat?.EtatHeure.Trim();
		public ZString Refdos => ReponseDeclaration?.Entete?.Refdos;
		public ZBool HasLiquidation => Notification != null && Notification.ReponseArticles.Any();
		public Collection<TReponseArticle> Liquidation => Notification?.ReponseArticles ?? new Collection<TReponseArticle>();
		public ZBool IsGvmsAdvice => false;
		public ZLong? Montantcautionnable => Notification?.LiquidationGen?.Montantcautionnable;
		public ZString Evenement => ZString.Empty;
		public ZBool HasErrors
		{
			get
			{
				var erreurGen = ReponseDeclaration?.ReponseDatas?.Erreur?.ErreurGen;
				return erreurGen != null && erreurGen.Any();
			}
		}

		public ZString mrnecs => ReponseDeclaration?.Entete?.Mrnecs;

		public IEnumerable<IResponseLiquidationWrapper> ResponseLiquidations
		{
			get
			{
				foreach (var responseArticle in Liquidation)
				{
					yield return new DeltaCExportResponseLiquidationWrapper(responseArticle);
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
							responseArticleDAUs.Add(new DeltaCExportResponseArticleDAUWrapper(articleDAU));
						}
					}
				}
				return responseArticleDAUs;
			}
		}

		List<IResponseArticleDAUWrapper> responseArticleDAUs;

		class DeltaCExportResponseLiquidationWrapper : IResponseLiquidationWrapper
		{
			public DeltaCExportResponseLiquidationWrapper(TReponseArticle responseArticle)
			{
				this.responseArticle = Argument.NotNull(responseArticle, nameof(responseArticle));
			}
			readonly TReponseArticle responseArticle;
			public ZShort numart => responseArticle.Numart;
			public IEnumerable<ITaxDetailWrapper> TaxDetails
			{
				get
				{
					foreach (var taxDetail in responseArticle.TTaxationDetails)
					{
						yield return new DeltaCExportTaxDetailWrapper(taxDetail);
					}
				}
			}
		}

		class DeltaCExportResponseArticleDAUWrapper : IResponseArticleDAUWrapper
		{
			public DeltaCExportResponseArticleDAUWrapper(TArticleDauExport articleDau)
			{
				this.articleDau = Argument.NotNull(articleDau, nameof(articleDau));
			}
			readonly TArticleDauExport articleDau;

			public ZShort numart => articleDau.Article?.Numart ?? 0;
			public ZDecimal? Valstat => articleDau?.Article?.DonneesFinancieres?.Valstat;
			public ZDecimal? Valdou => articleDau?.Article?.DonneesFinancieres?.Valdou;
			public ZDecimal? Asstva => ZDecimal.Zero;
		}

		class DeltaCExportTaxDetailWrapper : ITaxDetailWrapper
		{
			public DeltaCExportTaxDetailWrapper(TTaxationDetailReponse taxDetail)
			{
				this.taxDetail = Argument.NotNull(taxDetail, nameof(taxDetail));
			}
			readonly TTaxationDetailReponse taxDetail;
			public ZString typtax => taxDetail.Typtax;
			public ZString codtax => taxDetail.Codtax;
			public ZDecimal asstax => taxDetail.Asstax ?? decimal.Zero;
			public ZDecimal quotax => taxDetail.Quotax ?? decimal.Zero;
			public ZLong montanttax => taxDetail.Montanttax;
			public ZString statutLiquidation => taxDetail.StatutLiquidation;
			public ZString SuppUnitsMethodOfCalculation => LiquidationItemHasSuppUnits ? taxDetail.UniSpe.Unispe + taxDetail.UniSpe.Qualifunispe : string.Empty;
			public ZBool LiquidationItemHasSuppUnits => !string.IsNullOrEmpty(taxDetail.UniSpe?.Unispe);
		}

		ReponseDeclaration ReponseDeclaration => ResponseMessage?.ReponseDeclaration;
		TNotification Notification => ReponseDeclaration?.ReponseDatas?.Notification;
		TEtat notificationEtat => Notification?.Etat;
	}
}
