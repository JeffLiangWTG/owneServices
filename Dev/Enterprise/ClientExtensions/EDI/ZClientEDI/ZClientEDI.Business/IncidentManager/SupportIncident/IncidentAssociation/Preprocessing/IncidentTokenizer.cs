using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WTG.MachineLearning.NLP.Preprocessing;
using WTG.MachineLearning.Resources.NLP;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public interface IIncidentTokenizer
	{
		string IncidentToString(SupportIncident incident);

		IEnumerable<string> TokenizeIncident(string incidentString);
	}

	public class IncidentTokenizer : IIncidentTokenizer
	{
		public bool Synonymize { get; set; } = true;

		public IStemmer Stemmer { get; set; }

		readonly IDictionary<string, string> synonymTokens;

		readonly IReadOnlyList<string> omit = new List<string>() { ".", ",", ";", "?", "!", "\"", "'", ":", "#", "-", "_", "=", ":", "\\", "/", "(", ")", "[", "]", "<", ">", "{", "}", "`", "~" };

		IReadOnlyCollection<(string Search, string Replace)> replacementsPreProtect { get; } = new[]
		{
			(@"(?<= )[a-z0-9\+._-]+@[a-z0-9._-]+\.[a-z0-9_-]+(?= )", "emailtokenn"),
		};

		IReadOnlyCollection<(string Search, string Replace)> replacementsPostProtect { get; } = new[]
		{
			(@"(?<= )(0.5|50%|½|1\/2)(?= )", "half"),
			(@"(?<= )(0.25|25%|¼|1\/4)(?= )", "onequarter"),
			(@"(?<= )(0.75|75%|¾|3\/4)(?= )", "threequarters"),
			(@"(?<= )\d{4}[\/\-\.]\d{2}[\/\-\.]\d{2}([t ]\d{2}\:\d{2}(\:\d{2}(\.\d+)?)?)?([t ]\d{2}\:\d{2}(\:\d{2}(\.\d+)?)?)?(?= )", "datetokenn"),
			(@"(?<= )(\d{1,2}[\/\-\. ])?(jan|feb|mar(ch)?|apr(il)?|may|june?|july?|aug|sept?|oct|nov|dec)([\/\-\. ](\d{4}|\d{2})?([t ]\d{2}\:\d{2}(\:\d{2}(\.\d+)?)?)?)?(?= )", "datetokenn"),
			(@"(?<= )(127\.0{1,3}\.0{1,3}\.\d{1,3}|local.?host)(?= )", "localhost"),
			(@"(?<= )(\d{1,3}\.){3}\d{1,3}(?= )", "ipaddresstokenn"),
			(@"(?<= )((v(er(sion)?)?).?\d{1,4}(.\d+)*|\d+(\.\d+){2,})(?= )", "versiontokenn"),
			(@"(?<= )\~?\d+([\.\,]\d{3})*([\.\,]\d+)? ?[kmgt]i?b(?= )", "sizetokenn"),
			(@"(?<= )\d+(\,\d{3})*(st|nd|rd|th)(?= )", "ordinaltokenn"),
			(@"(?<= )(first|second|third|fourth|fifth|sixth|seventh|eighth|ninth|tenth|hundredth)(?= )", "ordinaltokenn"),
			(@"(?<= )\d{1,3}(\.\d+)?\%(?= )", "percentagetokenn"),
			(@"(?<= )(wi|work.?item).?\d{8}[\,\.\;\:]?(?= )", "workitemtokenn"),
			(@"(?<= )(?<= )(cs|((customer.?)?support.?)?.?(incident|issue|request)).?\d{8}[\,\.\;\:]?(?= )", "supportincidenttokenn"),
			(@"(?<= )(utf\-?(1|7|8|16|32)?|uni[\-\s]?code|ascii|ansi)(?= )", "encodingtokenn"),
			(@"(?<= )europe(an)?(?= )", "europe"),
			(@"(?<= )(america|united.?states|usa|u\.s\.a\.?|u\.s\.?)(?= )", "united states of america"),
			(@"(?<= )(united.?kingdom|uk)(?= )", "united kingdom"),
			(@"(?<= )((united.?)?arab.?)?emirates|uae(?= )", "united arab emirates"),
			(@"(?<= ).\s?net(?= )", "dotnet"),
			(@"(?<= )c\#(?= )", "csharp"),
			(@"(?<= )32.?bits?(?= )", "32bit"),
			(@"(?<= )64.?bits?(?= )", "64bit"),
			(@"(?<= )\d+(.\d+)?(px|pt|em)(?= )", "screensizetokenn"),
			(@"(?<= )(\+|\-)?\d+(?= )", "numbertokenn"),
			(@"(?<= )_?\d+(?= )", "numbertokenn"),
			(@"(?<= )\d{1,2}[ap]m(?= )", "timetokenn"),
			(@"(?<= )(0x)?([0-9a-f]{2}){2,}(?= )", "hextokenn"),
			(@"(?<= )(₼|₽|\$b|\$u|¢|£|¥|₱|﷼|₭|₦|₨|₩|₮|€|฿|₡|៛|؋|₴|₪|₫|br|bs|bz\$|c\$|chf|ft|gs|j\$|kč|km|kn|kr|lei|lek|mt|nt\$|r\$|rd\$|rm|rp|tt\$|z\$|zł|ден|лв|\$|aud|brl|cad|chf|cny|eur|gbp|hkd|idr|ils|jpy|mxn|nzd|sar|sgd|usd|vnd|zar).?\d+(\,\d{3})*(.\d+)?(?= )", "currencytokenn"),
			(@"(?<= )\d+(\,\d{3})*(.\d+)?.?(₼|₽|\$b|\$u|¢|£|¥|₱|﷼|₭|₦|₨|₩|₮|€|฿|₡|៛|؋|₴|₪|₫|br|bs|bz\$|c\$|chf|ft|gs|j\$|kč|km|kn|kr|lei|lek|mt|nt\$|r\$|rd\$|rm|rp|tt\$|z\$|zł|ден|лв|\$|aud|brl|cad|chf|cny|eur|gbp|hkd|idr|ils|jpy|mxn|nzd|sar|sgd|usd|vnd|zar)(?= )", "currencytokenn"),
			(@"[⁇⁈⁉‼‽]", "?"),
			(@"[–—―]+", " - "),
			(@"‚", ","),
			(@"[‘’`“”\""´]", "'"),
			(@"\-{2,}", " - "),
			(@"\={2,}", " = "),
			(@"\*{2,}", " * "),
			(@"\#{2,}", " # "),
			(@"_{2,}", " _ "),
			(@"\?+", " ? "),
			(@"\!+", " ! "),
			(@"\.+", " . "), // <-- from this point on, numbers with dots (e.g. 2.36) won't match
			(@",+", " , "),
			(@";+", " , "),
			(@"\(+", " ( "),
			(@"\)+", " ) "),
			(@"\[+", " [ "),
			(@"\]+", " ] "),
			(@"\<+", " < "),
			(@"\>+", " > "),
			(@"\{+", " { "),
			(@"\}+", " } "),
			(@"\:+", " : "),
			(@"\++", " + "),
			(@"(?<= )anti[\-\s]", "anti"),
			(@"(?<= )dis[\-\s]", "dis"),
			(@"(?<= )inter[\-\s]", "inter"),
			(@"(?<= )intra[\-\s]", "intra"),
			(@"(?<= )mis[\-\s]", "mis"),
			(@"(?<= )multi[\-\s]", "multi"),
			(@"(?<= )pre[\-\s]", "pre"),
			(@"(?<= )re[\-\s]", "re"),
			(@"(?<= )semi[\-\s]", "semi"),
			(@"(?<= )sub[\-\s]", "sub"),
			(@"(?<= )trans[\-\s]", "trans"),
			(@"(?<= )un[\-\s]", "un"),
			(@"[\-\s]ity(?= )", "ity"),
			(@"[\-\s]ism(?= )", "ism"),
			(@"[\-\s][ae]nce(?= )", "ence"),
			(@"[\-\s]ment(?= )", "ment"),
			(@"[\-\s]ness(?= )", "ness"),
			(@"[\-\s]i?fy(?= )", "ify"),
			(@"[\-\s]esque(?= )", "esque"),
			(@"[\-\s]i?ous(?= )", "ious"),
			(@"(?<= )should of(?= )", "should\'ve"),
			(@"(?<= )could of(?= )", "could\'ve"),
			(@"(?<= )would of(?= )", "would\'ve"),
			(@"(?<= )alot(?= )", "a lot"),
			(@"(?<= )abit(?= )", "a bit"),
			(@"(?<= )awhile(?= )", "a while"),
			(@"(?<= )anymore(?= )", "any more"),
			(@"(?<= )half.?way(?= )", "halfway"),
			(@"(?<= )ain\'?t(?= )", "am not"),
			(@"(?<= )aren\'?t(?= )", "are not"),
			(@"(?<= )can\'?t(?= )", "cannot"),
			(@"(?<= )\'?cos(?= )", "because"),
			(@"(?<= )could\'?ve(?= )", "could have"),
			(@"(?<= )couldn\'?t(?= )", "could not"),
			(@"(?<= )couldn\'?t\'?ve(?= )", "could not have"),
			(@"(?<= )didn\'?t(?= )", "did not"),
			(@"(?<= )doesn\'?t(?= )", "does not"),
			(@"(?<= )don\'?t(?= )", "do not"),
			(@"(?<= )g\'?day(?= )", "good day"),
			(@"(?<= )gimme(?= )", "give me"),
			(@"(?<= )gonna(?= )", "going to"),
			(@"(?<= )gotta(?= )", "got to"),
			(@"(?<= )hadn\'?t(?= )", "had not"),
			(@"(?<= )hasn\'?t(?= )", "has not"),
			(@"(?<= )haven\'?t(?= )", "have not"),
			(@"(?<= )he\'?d(?= )", "he would"),
			(@"(?<= )he\'ll(?= )", "he will"),
			(@"(?<= )he\'?s(?= )", "he is"),
			(@"(?<= )how\'?ll(?= )", "how will"),
			(@"(?<= )how\'?re(?= )", "how are"),
			(@"(?<= )how\'?s(?= )", "how is"),
			(@"(?<= )i\'?d(?= )", "i would"),
			(@"(?<= )i\'?ll(?= )", "i will"),
			(@"(?<= )i\'?m(?= )", "i am"),
			(@"(?<= )i\'?ve(?= )", "i have"),
			(@"(?<= )isn\'?t(?= )", "is not"),
			(@"(?<= )it\'?d(?= )", "it would"),
			(@"(?<= )it\'?ll(?= )", "it will"),
			(@"(?<= )it\'?s(?= )", "it is"),
			(@"(?<= )let\'?s(?= )", "let us"),
			(@"(?<= )may\'?ve(?= )", "may have"),
			(@"(?<= )mightn\'?t(?= )", "might not"),
			(@"(?<= )might\'?ve(?= )", "might have"),
			(@"(?<= )mustn\'?t(?= )", "must not"),
			(@"(?<= )mustn\'?t\'?ve(?= )", "must not have"),
			(@"(?<= )must\'?ve(?= )", "must have"),
			(@"(?<= )needn\'?t(?= )", "need not"),
			(@"(?<= )o\'?clock(?= )", "of the clock"),
			(@"(?<= )oughtn\'?t(?= )", "ought not"),
			(@"(?<= )shan\'?t(?= )", "shall not"),
			(@"(?<= )she\'?d(?= )", "she had"),
			(@"(?<= )she\'?ll(?= )", "she will"),
			(@"(?<= )she\'?s(?= )", "she is"),
			(@"(?<= )should\'?ve(?= )", "should have"),
			(@"(?<= )shouldn\'?t(?= )", "should not"),
			(@"(?<= )shouldn\'?t\'?ve(?= )", "should not have"),
			(@"(?<= )that\'?ll(?= )", "that will"),
			(@"(?<= )that\'?re(?= )", "that are"),
			(@"(?<= )that\'?s(?= )", "that is"),
			(@"(?<= )that\'?d(?= )", "that would"),
			(@"(?<= )there\'?d(?= )", "there had"),
			(@"(?<= )there\'?ll(?= )", "there will"),
			(@"(?<= )there\'?re(?= )", "there are"),
			(@"(?<= )there\'?s(?= )", "there is"),
			(@"(?<= )these\'?re(?= )", "these are"),
			(@"(?<= )they\'?d(?= )", "they would"),
			(@"(?<= )they\'?ll(?= )", "they will"),
			(@"(?<= )they\'?re(?= )", "they are"),
			(@"(?<= )they\'?ve(?= )", "they have"),
			(@"(?<= )those\'?re(?= )", "those are"),
			(@"(?<= )wasn\'?t(?= )", "was not"),
			(@"(?<= )we\'?d(?= )", "we would"),
			(@"(?<= )we\'?d\'?ve(?= )", "we would have"),
			(@"(?<= )we\'?ll(?= )", "we will"),
			(@"(?<= )we\'?re(?= )", "we are"),
			(@"(?<= )we\'?ve(?= )", "we have"),
			(@"(?<= )weren\'?t(?= )", "were not"),
			(@"(?<= )what\'?d(?= )", "what did"),
			(@"(?<= )what\'?ll(?= )", "what will"),
			(@"(?<= )what\'?re(?= )", "what are"),
			(@"(?<= )what\'?s(?= )", "what is"),
			(@"(?<= )what\'?ve(?= )", "what have"),
			(@"(?<= )when\'?s(?= )", "when is"),
			(@"(?<= )where\'?d(?= )", "where did"),
			(@"(?<= )where\'?re(?= )", "where are"),
			(@"(?<= )where\'?s(?= )", "where is"),
			(@"(?<= )where\'?ve(?= )", "where have"),
			(@"(?<= )who\'?d(?= )", "who would"),
			(@"(?<= )who\'?d\'?ve(?= )", "who would have"),
			(@"(?<= )who\'?ll(?= )", "who will"),
			(@"(?<= )who\'?re(?= )", "who are"),
			(@"(?<= )who\'?s(?= )", "who is"),
			(@"(?<= )who\'?ve(?= )", "who have"),
			(@"(?<= )why\'?d(?= )", "why did"),
			(@"(?<= )why\'?re(?= )", "why are"),
			(@"(?<= )why\'?s(?= )", "why"),
			(@"(?<= )won\'?t(?= )", "will not"),
			(@"(?<= )would\'?ve(?= )", "would have"),
			(@"(?<= )wouldn\'?t(?= )", "would not"),
			(@"(?<= )y\'?all(?= )", "you all"),
			(@"(?<= )you\'?d(?= )", "you had"),
			(@"(?<= )you\'?ll(?= )", "you will"),
			(@"(?<= )you\'?re(?= )", "you are"),
			(@"(?<= )you\'?ve(?= )", "you have"),
			(@"\'(s|t)(?= )", " "),
			(@"[\/\\'\""]", " "),
			(@"(?<= )wise.?tech(?= )", "wtg"),
			(@"(?<= )wtg.?global(?= )", "wtg"),
			(@"(?<= )wtg.?team(?= )", "wtg"),
			(@"(?<= )wise.?cloud(?= )", "wisecloud"),
			(@"(?<= )wise.?rates(?= )", "wiserates"),
			(@"(?<= )wise.?learn(ing)?(?= )", "wiselearning"),
			(@"(?<= )(cargo.?wise|cw.?1|c1|cwo|cw)(?= )", "cargowise"),
			(@"(?<= )cargowise.?(1|one)(?= )", "cargowise"),
			(@"(?<= )(border.?wise|edi.?tarr?iff?)(?= )", "borderwise"),
			(@"(?<= )service.?tasks?(?= )", "servicetask"),
			(@"(?<= )work.?items?(?= )", "workitem"),
			(@"(?<= )e.?doc(ument)?s?(?= )", "edocs"),
			(@"(?<= )e.?conversations?(?= )", "econversation"),
			(@"(?<= )trouble.?shoot(ing)?(?= )", "troubleshoot"),
			(@"(?<= )user.?names?(?= )", "username"),
			(@"(?<= )(pass.?word|pwd|passwd|passwrd)(?= )", "password"),
			(@"(?<= )(sign|log).?ins?(?= )", "login"),
			(@"(?<= )(sign|log).?out(?= )", "logout"),
			(@"(?<= )sys(?= )", "system"),
			(@"(?<= )(ms|mso?ft)(?= )", "microsoft"),
			(@"(?<= )(db|data.?base)(?= )", "database"),
			(@"(?<= )entity.?framework(?= )", "entityframework"),
			(@"(?<= )how.?to(?= )", "how to"),
			(@"(?<= )(cell|tele|mobile).?phone(?= )", "phone"),
			(@"(?<= )web.?servers?(?= )", "webserver"),
			(@"(?<= )screen.?shots?(?= )", "screenshot"),
			(@"(?<= )print.?screen(?= )", "printscreen"),
			(@"(?<= )sub.?tabs?(?= )", "subtab"),
			(@"(?<= )plug.?ins?(?= )", "plugin"),
			(@"(?<= )admin(istrator)?s?(?= )", "administrator"),
			(@"(?<= )(os|operating.?systems?)(?= )", "operatingsystem"),
			(@"(?<= )active.?directory(?= )", "activedirectory"),
			(@"(?<= )((all?ways.?)?on.?top|top.?most)(?= )", "topmost"),
			(@"(?<= )web.?pages?(?= )", "webpage"),
			(@"(?<= )web.?sites?(?= )", "website"),
			(@"(?<= )host.?names?(?= )", "hostname"),
			(@"(?<= )help.?desks?(?= )", "helpdesk"),
			(@"(?<= )white.?space(?= )", "whitespace"),
			(@"(?<= )white.?list(?= )", "whitelist"),
			(@"(?<= )black.?list(?= )", "blacklist"),
			(@"(?<= )shel(f|ve).?sets?(?= )", "shelveset"),
			(@"(?<= )check.?ins?(?= )", "checkin"),
			(@"(?<= )check.?out(?= )", "checkout"),
			(@"(?<= )ctors?(?= )", "constructor"),
			(@"(?<= )one.?drive(?= )", "onedrive"),
			(@"(?<= )google.?drive(?= )", "googledrive"),
			(@"(?<= )drop.?box(?= )", "dropbox"),
			(@"(?<= )left.?click(ed|ing)?(?= )", "leftclick"),
			(@"(?<= )right.?click(ed|ing)?(?= )", "rightclick"),
			(@"(?<= )single.?click(ed|ing)?(?= )", "singleclick"),
			(@"(?<= )double.?click(ed|ing)?(?= )", "doubleclick"),
			(@"(?<= )e.?mail(s|ed|er|ing)?(?= )", "email"),
			(@"(?<= )discs?(?= )", "disk"),
			(@"(?<= )f([1-9]|10|11|12)(?= )", "functionkey"),
			(@"(?<= )licen[cs](e[sd]?|ing)(?= )", "licence"),
			(@"(?<= )(pls|plz)(?= )", "please"),
			(@"(?<= )(thank.?you|thanx|thx)(?= )", "thanks"),
			(@"(?<= )(hte|teh)(?= )", "the"),
			(@"(?<= )(hi|good (day|morning|afternoon|evening)|hey|hallo)(?= )", "hello"),
			(@"(?<= )(\.\s){2,}(?= )", " . "),
			(@"(?<= )\&+(?= )", "and"),
			(@"\s+", " "),
		};

		readonly List<string> names = English.CommonNames.ToList();

		readonly List<string> stopwords = English.StopWords
			.Concat(new[] { "hello", "please", "thanks" })
			.ToList();

		public IncidentTokenizer(IDictionary<string, string> synonymTokens, IStemmer stemmer)
		{
			this.synonymTokens = synonymTokens ?? throw new ArgumentNullException(nameof(synonymTokens));

			Stemmer = stemmer;
		}

		public IncidentTokenizer(IStemmer stemmer)
		{
			Stemmer = stemmer;
			Synonymize = false;
		}

		public string ProtectTokens(string str, IDictionary<string, string> protectedTokens)
		{
			var tokensToProtect = new List<string>()
			{
				@"<( )*!--(?!-->).*?--( )*>|<[^<>\/]*?\/>|<(?<tagLable>(\w( )*)*)[^>]*?>.*?<\/(\k<tagLable>)>", // XML
				@"((ftp|https?):)?(\/\/)?(www\.)?[-a-zA-Z0-9@:%._\+~#=\*]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&\/=\*]*)", // URLs
				@"[{(]?[0-9a-f]{8}[-]?(?:[0-9a-f]{4}[-]?){3}[0-9a-f]{12}[)}]?" // GUIDs
			};

			foreach (var tokenToProtect in tokensToProtect)
			{
				str = Regex.Replace(str, tokenToProtect, match =>
				{
					string key = string.Format(CultureInfo.InvariantCulture, "protectedtokenn{0}", protectedTokens.Count + 1);
					protectedTokens.Add(key, match.ToString());
					return $" {key} ";
				});
			}

			return str;
		}

		public string ApplyReplacementsPreProtect(string str)
		{
			return ApplyReplacements(str, replacementsPreProtect);
		}

		public string ApplyReplacementsPostProtect(string str)
		{
			return ApplyReplacements(str, replacementsPostProtect);
		}

		string ApplyReplacements(string str, IEnumerable<(string Search, string Replace)> regexReplacements)
		{
			foreach (var tup in regexReplacements)
			{
				str = Regex.Replace(str, tup.Search, tup.Replace);
			}

			return str;
		}

		public string TransformCountries(string str)
		{
			foreach (var country in WTG.MachineLearning.Resources.NLP.English.Countries)
			{
				str = Regex.Replace(str, $"(?<= ){country}(?= )", "countrytokenn");
			}

			return str;
		}

		public string StripTrainingLinks(string str)
		{
			return Regex.Replace(str, @"(https?:\/\/(www\.)?myaccount-portal\.cargowise\.com\/my-account\/documents\/userguides\/workbooks\/)?\d?[a-z]{3}\d{3}(\.pdf)?", match =>
			{
				var mStr = match.ToString();
				mStr = Regex.Replace(mStr, @"https?:\/\/(www\.)?myaccount-portal\.cargowise\.com\/my-account\/documents\/userguides\/workbooks\/", string.Empty);
				return Regex.Replace(mStr, @"\.pdf", string.Empty);
			});
		}

		public string StripUpdateNotes(string str)
		{
			return Regex.Replace(str, @"(https?:\/\/(www\.)?(myaccount-portal\.)?cargowise\.com\/(my-account\/)?documents\/updatenotes\/\S{0,20}updatenote\d{8}[a-z]?(\.pdf)?)|(\s\S{0,20}updatenote\d{8}[a-z]?(\.pdf)?)", match =>
			{
				var mStr = match.ToString();
				mStr = Regex.Replace(mStr, @"https?:\/\/(www\.)?(myaccount-portal\.)?cargowise\.com\/(my-account\/)?documents\/updatenotes\/", string.Empty);
				mStr = Regex.Replace(mStr, @"\d{8}[a-z]?(\.pdf)?", string.Empty);
				return $" {mStr}tokenn ";
			});
		}

		public string TransformTrainingTokens(string str)
		{
			return Regex.Replace(str, @"\s\d?[a-z]{3}\d{3}\s", match =>
			{
				var mStr = match.ToString();
				var module = mStr.Length == 8 ? mStr.Substring(1, 3) : mStr.Substring(2, 3);
				return $" {module}trainingtokenn ";
			});
		}

		public string RemoveHyphens(string str)
		{
			return Regex.Replace(str, @"[a-zA-Z]{2,}(\-[a-zA-Z]{2,})+", match =>
			{
				return Regex.Replace(match.ToString(), @"\-", string.Empty);
			});
		}

		public string IncidentToString(SupportIncident incident)
		{
			var pieces = new List<string>()
			{
				Regex.Replace(string.Format(CultureInfo.InvariantCulture,"{0}productcode",incident.IM_Product), @"[\s&]", string.Empty),
				Regex.Replace(string.Format(CultureInfo.InvariantCulture,"{0}programarea",incident.IM_ProgramArea), @"[\s&]", string.Empty),
				Regex.Replace(string.Format(CultureInfo.InvariantCulture,"{0}module",incident.IM_Module), @"[\s&]", string.Empty),
				Regex.Replace(string.Format(CultureInfo.InvariantCulture,"{0}criticality",incident.IM_Priority), @"[\s&]", string.Empty),
				Regex.Replace(string.Format(CultureInfo.InvariantCulture,"{0}country",incident.IM_RN_NKCountry), @"[\s&]", string.Empty),
				incident.IM_Description,
				incident.DetailNoteText
			};

			foreach (var message in incident.EConversation.GetTimeOrderedMessages().Where(jcm => !jcm.IsSystemMessage))
			{
				pieces.Add(message.Body);
			}

			foreach (NewWorkItem workItem in incident.RelatedWorkItems)
			{
				pieces.Add(workItem.WKI_Summary);
			}

			return string.Join(" ", pieces);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		public IEnumerable<string> TokenizeIncident(string incidentString)
		{
			var str = incidentString.Trim().ToLowerInvariant();
			// replace non-ASCII characters with "?"
			byte[] suspectBytes = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, Encoding.Unicode.GetBytes(str));
			str = Encoding.ASCII.GetString(suspectBytes);

			str = Regex.Replace(str, @"\s+", " ");

			str = StripTrainingLinks(str);
			str = StripUpdateNotes(str);

			str = ApplyReplacements(str, replacementsPreProtect);

			var protectedTokens = new Dictionary<string, string>();
			str = ProtectTokens(str, protectedTokens);

			str = $" {str} "; // certain replacements won't work without this

			str = Regex.Replace(str, $"(?<= )({string.Join("|", names)})(?= )", " nametokenn ");
			str = Regex.Replace(str, " (nametokenn ){2,}", " nametokenn ");

			str = ApplyReplacements(str, replacementsPostProtect);

			str = TransformCountries(str);
			str = TransformTrainingTokens(str);
			str = RemoveHyphens(str);

			var words = str.Split(' ')
					.Where(s => !stopwords.Contains(s))
					.Where(s => !omit.Contains(s))
					.Where(s => !string.IsNullOrWhiteSpace(s));

			if (Stemmer != null)
			{
				words = Stemmer.StemWords(words);
			}

			words = words.Select(s => protectedTokens.ContainsKey(s) ? protectedTokens[s] : s);

			return Synonymize ? words
				.Select(s => synonymTokens.ContainsKey(s) ? synonymTokens[s] : null)
				.Where(s => !string.IsNullOrWhiteSpace(s)) : words;
		}
	}
}
