using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// A helper class to determine the types of barcodes
	/// </summary>
	public static class BarcodeHelper
	{
		/* Here be barcodes: A primer on barcode validity in cargowise
		 * 
		 * There are a few different barcode formats floating around, and as it took me a while to reverse engineer what is valid, here is my learnings so no one 
		 * else has to go through my pain. See the valid formats below.
		 * A = Alphabet characters only
		 * B = Alphanumeric
		 * 
		 * 1. Ref type only
		 * Example:	^SHP=123456|
		 * Format:	^AAA=BBBBBB|
		 * SHP or CON with a reference to an object id
		 * 
		 * 2. Doc Type only
		 * Example:	^DOC=CIV|
		 * Format:	^DOC=AAA|
		 * DOC type code specifying document type
		 * 
		 * 3. Combined ref and doc type:
		 * Example:	^SHP=12345;CIV|
		 * Format:	^AAA=BBBBB;AAA|
		 * The reference type, semicolon, then document type
		 * 
		 * 4. Ref type with client
		 * Example: ^DDR=00001636;DDR@MFH|
		 * A ref type barcode (with or without doc type) can also include a client specific code with @AAA just before the end
		 * 
		 * 5. PlaceHolder Barcode
		 * Example Placeholder: ^DDR=00001636|
		 * Example non placeholder: ^DDR=00001636;|
		 * If a doc type barcode ends in ;|, it is not a placeholder and the page should be saved
		 * if it ends in | without the semicolon, it is a placeholder and doesnt need to be saved
		 * 
		 * 6. Shipment Barcode
		 * Example: [SHAHBLPVGFMOSHA00001980]
		 * Format: [AAABBBCCCDDDS+]
		 * New format created more recently
		 * Barcode must start with [ and end with ], have all 12 start characters (AAA = 3-letter Company, BBB = 3-letter DocType, 
		 * CCC = 3-letter Origin, DDD = 3-letter Destination) and a minimum 3 letter length housebill.
		*/

		/// <summary>
		/// e.g. an exclusively shipment type format barcode, more to follow
		/// </summary>
		public static bool IsReferenceTypeBarcode(string barcode)
		{
			return IsShipmentBarcode(barcode);
		}

		/// <summary>
		/// Whether or not the barcode is an original-style DocType barcode (starts with ^, ends with |)
		/// </summary>
		public static bool IsDocTypeBarcode(string barcode)
		{
			return barcode.StartsWith("^", StringComparison.OrdinalIgnoreCase) && barcode.EndsWith("|", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// A document type barcode that contains only RefType information, e.g. SHP=123
		/// </summary>
		public static bool IsRefTypeOnlyBarcode(string barcode)
		{
			return IsDocTypeBarcode(barcode) && IsRefTypeEncoded(barcode) && barcode.IndexOf(';') < 0;
		}

		/// <summary>
		///  A Document type barcode that contains only DocType information, e.g. DOC=CIV
		/// </summary>
		public static bool IsDocTypeOnlyBarcode(string barcode)
		{
			return IsDocTypeBarcode(barcode) && !IsRefTypeEncoded(barcode);
		}

		/// <summary>
		/// A Document type barcode that contains both RefType and Doctype info e.g. SHP=123;CIV
		/// </summary>
		public static bool IsDocTypeAndRefTypeBarcode(string barcode)
		{
			return IsDocTypeBarcode(barcode) && IsRefTypeEncoded(barcode) && barcode.IndexOf(';') > 0;
		}

		/// <summary>
		/// Whether or not the barcode is a newstyle shipment barcode (Starts with [, ends with ])
		/// Dedicated prefix and suffix and holds information relating only to shipments, e.g.
		/// Housebill, Masterbill
		/// </summary>
		public static bool IsShipmentBarcode(string barcode)
		{
			return barcode.StartsWith("[", StringComparison.OrdinalIgnoreCase) && barcode.EndsWith("]", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Whether or not the barcode contains company code used for filtering accounting documents
		/// </summary>
		public static bool IsCompanySpecificBarcode(string barcode)
		{
			return IsDocTypeBarcode(barcode) && barcode.Contains("@");
		}

		/// <summary>
		/// Is this barcode a recognised Enterprise barcode type?
		/// </summary>
		public static bool IsValidBarcodeType(string barcode)
		{
			return IsDocTypeBarcode(barcode) || IsShipmentBarcode(barcode);
		}

		/// <summary>
		/// Is this a placeholder barcode - a barcode on a page that doesn't need to be saved?
		/// </summary>
		public static bool IsPlaceholderBarcode(string barcode)
		{
			return IsDocTypeBarcode(barcode) && (barcode[barcode.Length - 2] != ';');
		}

		/// <summary>
		/// Is a reference type encoded in this barcode somewhere?
		/// </summary>
		public static bool IsRefTypeEncoded(string barcode)
		{
			return !EncodedRefType(barcode).IsEmpty || IsReferenceTypeBarcode(barcode);
		}

		/// <summary>
		/// Is a DocType encoded in this barcode somewhere?
		/// </summary>
		public static bool IsDocTypeEncoded(string barcode)
		{
			return IsDocTypeOnlyBarcode(barcode) || IsDocTypeAndRefTypeBarcode(barcode) || IsShipmentBarcode(barcode);
		}

		public static ZString GetValueForCode(string barcode, ZString code)
		{
			string[] barcodeParts = ParseBarcodeParts(barcode);
			ZString foundValue = ZString.Empty;

			if (barcodeParts != null)
			{
				foreach (ZString barcodePart in barcodeParts)
				{
					foundValue = ExtractValueForCode(barcodePart, code);
					if (!foundValue.IsEmpty)
					{
						break;
					}
				}
			}
			return foundValue;
		}

		static string[] ParseBarcodeParts(string barcode)
		{
			string[] barcodeParts = null;

			if (IsValidBarcodeType(barcode))
			{
				string codeString = barcode.Substring(1, barcode.Length - 2);
				barcodeParts = codeString.Split(';');
			}

			return barcodeParts;
		}

		/// <summary>
		/// Gets the RefType encoded within the barcode text. Use ONLY FOR DocType barcodes!
		/// </summary>
		public static ZString EncodedRefType(string barcode)
		{
			ZString returnValue = ZString.Empty;
			string[] barcodeParts = ParseBarcodeParts(barcode);

			returnValue = GetValueForCode(barcode, Core.Constants.FileFormats.REF);

			if (barcodeParts != null)
			{
				foreach (string docManagerCode in AssemblyDataLookup.GetDocManagerCodes())
				{
					if (!GetValueForCode(barcode, docManagerCode).IsEmpty)
					{
						returnValue = docManagerCode;
					}
				}
			}

			return returnValue;
		}

		/// <summary>
		/// Returns the portion after the = in a barcode value e.g. SHP=123 would return 123
		/// </summary>
		static ZString ExtractValueForCode(string barCodeData, string code)
		{
			code = code + "=";
			if (barCodeData.StartsWith(code, StringComparison.OrdinalIgnoreCase))
			{
				return barCodeData.Substring(barCodeData.IndexOf(code, StringComparison.OrdinalIgnoreCase) + code.Length);
			}
			else
			{
				return ZString.Empty;
			}
		}

		/// <summary>
		/// Takes a list of barcode strings and joins them together with " , ", but only joins the valid formatted barcodes
		/// Does not grow larger than maxlength, if it is specified
		/// </summary>
		public static string JoinValidBarcodes(IEnumerable<string> scannedBarcodes, int maxLength = int.MaxValue)
		{
			var stringBuilder = new StringBuilder();

			foreach (var barcode in scannedBarcodes.Where(x => BarcodeHelper.IsValidBarcodeType(x)))
			{
				if (stringBuilder.Length == 0)
				{
					if (barcode.Length < maxLength)
					{
						stringBuilder.Append(barcode);
					}
					else
					{
						break;
					}
				}
				else
				{
					if (stringBuilder.Length + barcode.Length + 3 < maxLength)
					{
						stringBuilder.Append(" , ");
						stringBuilder.Append(barcode);
					}
					else
					{
						break;
					}
				}
			}

			return stringBuilder.ToString();
		}
	}
}
