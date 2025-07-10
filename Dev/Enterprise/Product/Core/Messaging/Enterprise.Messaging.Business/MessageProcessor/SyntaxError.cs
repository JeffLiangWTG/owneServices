using CargoWise.Types;

namespace Enterprise.Messaging.MessageProcessors
{
	public class SyntaxError
	{
		public readonly ZString Code;
		public readonly ZString Description;

		public SyntaxError(ZString code)
		{
			#region SuppressResourceStringsCheckRegion

			switch (code)
			{
				case "2": Description = "Syntax version or level not supported"; break;
				case "7": Description = "Interchange recipient not actual recipient"; break;
				case "12": Description = "Invalid value"; break;
				case "13": Description = "Missing"; break;
				case "14": Description = "Value not supported in this position"; break;
				case "15": Description = "Not supported in this position"; break;
				case "16": Description = "Too many constituents"; break;
				case "17": Description = "No agreement"; break;
				case "18": Description = "Unspecified error"; break;
				case "19": Description = "Invalid decimal notation"; break;
				case "20": Description = "Character invalid as service character"; break;
				case "21": Description = "Invalid character(s)"; break;
				case "22": Description = "Invalid service character(s)"; break;
				case "23": Description = "Unknown Interchange sender"; break;
				case "24": Description = "Too old"; break;
				case "25": Description = "Test indicator not supported"; break;
				case "26": Description = "Duplicate detected"; break;
				case "27": Description = "Security function not supported"; break;
				case "28": Description = "Reference do not match"; break;
				case "29": Description = "Control count does not match number of instances received"; break;
				case "30": Description = "Functional groups and messages mixed"; break;
				case "31": Description = "More than one message type in group"; break;
				case "32": Description = "Lower level empty"; break;
				case "33": Description = "Invalid occurrence outside message or functional group"; break;
				case "34": Description = "Nesting indicator not allowed"; break;
				case "35": Description = "Too many segment repetitions"; break;
				case "36": Description = "Too many segment group repetitions"; break;
				case "37": Description = "Invalid type of character(s)"; break;
				case "38": Description = "Missing digit in front of decimal sign"; break;
				case "39": Description = "Data element too long"; break;
				case "40": Description = "Data element too short"; break;
				case "41": Description = "Permanent communication network error"; break;
				case "42": Description = "Temporary communication network error"; break;
				case "43": Description = "Unknown interchange recipient"; break;
				default: Description = "UNKNOWN"; break;
			}

			#endregion
		}
	}
}
