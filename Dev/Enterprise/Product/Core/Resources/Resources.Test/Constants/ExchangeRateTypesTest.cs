using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class ExchangeRateTypesTest : TestCase
	{
		public void TestExchangeRateTypesIsInvertedOrNotWhenCompanyIsReciprocalFlagIsChanged()
		{
			var rateTypesThatAreReviewedByProductTeamForInversion = new[] { "BUY","SEL","GCB","PER","IAT", "CUS", "CUD","CUE", "RMS", "RMB",
				"C01","C02","C03","C04","C05","C06","C07","C08","C09","C10",
				"C11","C12","C13","C14","C15","C16","C17","C18","C19","C20",
				"C21","C22","C23","C24","C25","C26","C27","C28","C29","C30",
				"C31","C32","C33","C34","C35","C36","C37","C38","C39","C40",
				"C41","C42","C43","C44","C45","C46","C47","C48","C49","C50",
				"C51","C52","C53","C54","C55","C56","C57","C58","C59","C60",
				"C61","C62","C63","C64","C65","C66","C67","C68","C69","C70",
				"C71","C72","C73","C74","C75","C76","C77","C78","C79","C80",
				"C81","C82","C83","C84","C85","C86","C87","C88","C89","C90",
				"C91","C92","C93","C94","C95","C96","C97","C98","C99",
				"L01","L02","L03","L04","L05","L06","L07","L08","L09","L10",
				"L11","L12","L13","L14","L15","L16","L17","L18","L19","L20",
				"L21","L22","L23","L24","L25","L26","L27","L28","L29","L30",
				"L31","L32","L33","L34","L35","L36","L37","L38","L39","L40",
				"L41","L42","L43","L44","L45","L46","L47","L48","L49","L50",
				"L51","L52","L53","L54","L55","L56","L57","L58","L59","L60",
				"L61","L62","L63","L64","L65","L66","L67","L68","L69","L70",
				"L71","L72","L73","L74","L75","L76","L77","L78","L79","L80",
				"L81","L82","L83","L84","L85","L86","L87","L88","L89","L90",
				"L91","L92","L93","L94","L95","L96","L97","L98","L99" };

			var allRateTypes = typeof(Constants.ExchangeRateTypes.Code).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
				.Select(fieldInfo => (string)fieldInfo.GetValue(null));

			var developerMessage = @"
New Exchange Rate Type(s) have been added to Constants.ExchangeRateTypes.
By default, any exchange rate with these new Rate Type(s) will be inverted when company 'Is Reciprocal' flag is changed.

If you want to keep this default behaviour, please add these new Rate Type(s) to 'rateTypesThatAreReviewedByProductTeamForInversion' array in this unit test.

If you want to avoid inverting of exchange rate when company 'Is Reciprocal' flag is changed, then please do the following.
1. Modify 'updateReciprocalExchangeRates' SQL procedure, so that these new Rate Type(s) are excluded from inversion
2. Add these new Rate Type(s) to 'rateTypesThatAreReviewedByProductTeamForInversion' array in this unit test.";

			AssertContainsExactElementsInAnyOrder(developerMessage, rateTypesThatAreReviewedByProductTeamForInversion, allRateTypes);
		}
	}
}
