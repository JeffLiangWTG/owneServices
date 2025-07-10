using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Response.Import;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DeltaDImportResponseMessageDataObject : MessageDataObject<TMessage>, IResponseDeltaDImportDataProvider
	{
		public DeltaDImportResponseMessageDataObject(FREDIMessage message)
			: base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new DeltaDImportResponsePrettier(this);
		}
		public ZBool IsImport => true;
		public ZString TransactionID => ResponseMessage?.EnveloppeMessage.TransactionId;
		public ZString Refdec => ReponseDeclaration?.Entete?.Refdec;
		public ZString Etat => notificationEtat?.Etat;
		public ZString EtatECS => ZString.Empty;
		public ZString EtatDate => notificationEtat?.EtatDate.Trim();
		public ZString EtatHeure => notificationEtat?.EtatHeure.Trim();
		public ZString Refdos => ReponseDeclaration?.Entete?.Refdos;
		public ZBool HasLiquidation => Notification != null && Notification.ReponseArticles.Any();
		public Collection<TLiquidationArt> Liquidation => Notification?.ReponseArticles ?? new Collection<TLiquidationArt>();
		public ZLong? Montantcautionnable => Notification?.LiquidationGen?.Montantcautionnable;
		public ZString Evenement => notificationEtat?.Evenement;

		public ZBool IsGvmsAdvice
		{
			get
			{
				var etat = notificationEtat;
				return etat != null && etat.Etat == EntryActionCodeList.Codes.ANT && etat.Evenement.Contains((NoResString)"Embarquement Transmanche");
			}
		}

		public ZBool HasErrors
		{
			get
			{
				var erreurGen = ReponseDeclaration?.ReponseDatas?.Erreur?.ErreurGen;
				return erreurGen != null && erreurGen.Any();
			}
		}

		public IEnumerable<IResponseLiquidationWrapper> ResponseLiquidations
		{
			get
			{
				foreach (var responseArticle in Liquidation)
				{
					yield return new DeltaDImportResponseLiquidationWrapper(responseArticle);
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
					if (Notification?.DatasImport?.ArticleDaUs != null)
					{
						foreach (var articleDAU in Notification.DatasImport.ArticleDaUs)
						{
							responseArticleDAUs.Add(new DeltaDImportResponseArticleDAUWrapper(articleDAU));
						}
					}
				}
				return responseArticleDAUs;
			}
		}

		List<IResponseArticleDAUWrapper> responseArticleDAUs;

		class DeltaDImportResponseLiquidationWrapper : IResponseLiquidationWrapper
		{
			public DeltaDImportResponseLiquidationWrapper(TLiquidationArt responseArticle)
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
						yield return new DeltaDImportTaxDetailWrapper(taxDetail);
					}
				}
			}
		}

		class DeltaDImportResponseArticleDAUWrapper : IResponseArticleDAUWrapper
		{
			public DeltaDImportResponseArticleDAUWrapper(TArticlesDauArticleDau articleDau)
			{
				this.articleDau = Argument.NotNull(articleDau, nameof(articleDau));
			}
			readonly TArticlesDauArticleDau articleDau;

			public ZShort numart => articleDau.Article?.Numart ?? 0;
			public ZDecimal? Valstat => articleDau?.Article?.DonneesFinancieres?.Valstat;
			public ZDecimal? Valdou => articleDau?.Article?.DonneesFinancieres?.Valdou;
			public ZDecimal? Asstva => articleDau?.Article?.DonneesFinancieres?.Asstva;
		}

		class DeltaDImportTaxDetailWrapper : ITaxDetailWrapper
		{
			public DeltaDImportTaxDetailWrapper(TTaxationDetail taxDetail)
			{
				this.taxDetail = Argument.NotNull(taxDetail, nameof(taxDetail));
			}
			readonly TTaxationDetail taxDetail;
			public ZString typtax => taxDetail.Typtax;
			public ZString codtax => taxDetail.Codtax;
			public ZDecimal asstax => taxDetail.Asstax ?? ZDecimal.Zero;
			public ZDecimal quotax => taxDetail.Quotax ?? ZDecimal.Zero;
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
