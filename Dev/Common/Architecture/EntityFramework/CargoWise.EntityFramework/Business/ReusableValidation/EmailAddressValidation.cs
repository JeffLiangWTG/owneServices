using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class EmailAddressValidation : ValidationProvider
	{
		public static void ValidateEmailAddress(ZPropertyInfo propertyInfo)
		{
			ValidateEmailAddress(propertyInfo, ZString.Empty);
		}

		public static void ValidateEmailAddress(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			if (!IsEmailAddressValid((ZString)propertyInfo.Value))
			{
				string prefix = Grammar.Instance.IndefiniteArticlePrefix(propertyDescription);
				string error = Res.GetString("268c60ba-32b1-4292-98cd-2246c385c510", "Email Address is not valid {0}{1}.", prefix, propertyDescription);

				propertyInfo.AddError(error);
			}
		}

		public static bool ValidateEmailAddress(ZString emailAddress, SchemaColumn emailAddressColumn, ZPropertyInfo parentPropertyInfo)
		{
			var result = true;
			if (emailAddressColumn != null)
			{
				var maxLength = emailAddressColumn.MaxLength;
				if (emailAddress.Length > maxLength)
				{
					parentPropertyInfo.AddError(Res.GetString("5DF214E3-BD33-46BA-A530-6AB0D614C526", "The email address \"{0}\" exceeds a maximum of {0} characters in length. You can separate multiple email addresses with a comma (,).", emailAddress, maxLength));
					result = false;
				}
			}
			if (!IsEmailAddressValid(emailAddress))
			{
				parentPropertyInfo.AddError(Res.GetString("5DB934AF-54E3-4E16-9B77-5C4A3021BC1D", "The email address \"{0}\" is invalid. You can separate multiple email addresses with a comma (,).", emailAddress));
				result = false;
			}
			return result;
		}

		public static bool ValidateEmailAddressesAsString(ZPropertyInfo emailAddressesAsStringPropertyInfo, SchemaColumn emailAddressColumn)
		{
			var emailAddressesAsString = (ZString)emailAddressesAsStringPropertyInfo.Value;
			var emailAddresses = emailAddressesAsString.Trim().ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ea => ea.Trim()).ToArray();
			return emailAddresses.All(emailAddress => ValidateEmailAddress(emailAddress, emailAddressColumn, emailAddressesAsStringPropertyInfo));
		}

		public static bool IsEmailAddressValid(ZString emailAddressToValidate)
		{
			bool result = true;

			if (!emailAddressToValidate.IsEmpty)
			{
				string strRegex =
					(NoResString)@"^((?>[a-zA-Z\d!#$%&'*+\-/=?^_`{|}~]+\x20*" +
					(NoResString)@"|""((?=[\x01-\x7f])[^""\\]|\\[\x01-\x7f])*""\x20*)*" +
					(NoResString)@"(?<angle><))?" +
					(NoResString)@"((?!\.)(?>\.?[a-zA-Z\d!#$%&'*+\-/=?^_`{|}~]+)+" +
					(NoResString)@"|""((?=[\x01-\x7f])[^""\\]|\\[\x01-\x7f])*"")" +
					(NoResString)@"@" +
					(NoResString)@"(((?!-)[a-zA-Z\d\-]+(?<!-)\.)+[a-zA-Z]{2,}" +
					(NoResString)@"|\[" +
					(NoResString)@"(((?(?<!\[)\.)(25[0-5]|2[0-4]\d|[01]?\d?\d)){4}" +
					(NoResString)@"|[a-zA-Z\d\-]*[a-zA-Z\d]:" +
					(NoResString)@"((?=[\x01-\x7f])[^\\\[\]]|\\[\x01-\x7f])+)" +
					(NoResString)@"\])" +
					(NoResString)@"(?(angle)>)$"; // Regular Expression
				Regex re = new Regex(strRegex);
				result = re.IsMatch(emailAddressToValidate);
			}

			return result;
		}

		public static bool IsEmailAddressValidAndNotEmpty(ZString emailAddressToValidate)
		{
			return (!emailAddressToValidate.Trim().IsEmpty && IsEmailAddressValid(emailAddressToValidate));
		}
	}
}
