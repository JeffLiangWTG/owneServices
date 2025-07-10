using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	#region SuppressResourceStringsCheckRegion

	public class SyntaxAndFormattingWrapperCollection : GenericWrapperCollection<SyntaxAndFormattingWrapper>
	{
		public SyntaxAndFormattingWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AddNew(CommandDefs._1_UsingFormatOnADataSource_Title
				 , CommandDefs._1_UsingFormatOnADataSource_Body);
			AddNew(CommandDefs._2_FormatCodes_Title
				 , CommandDefs._2_FormatCodes_Body);
			AddNew(CommandDefs._3_StringFormatCodes_Title
				 , CommandDefs._3_StringFormatCodes_Body);
			AddNew(CommandDefs._4_DateFormatCodes_Title
				 , CommandDefs._4_DateFormatCodes_Body);
			AddNew(CommandDefs._5_BooleanFormatCodes_Title
				 , CommandDefs._5_BooleanFormatCodes_Body);
			AddNew(CommandDefs._6_NumericFormatCodes_Title
				 , CommandDefs._6_NumericFormatCodes_Body);
			AddNew(CommandDefs._7_UsingFormatOnADataSourceCollection_Title
				 , CommandDefs._7_UsingFormatOnADataSourceCollection_Body);
			AddNew(CommandDefs._8_UsingTotalOnADataSourceCollection_Title
				 , CommandDefs._8_UsingTotalOnADataSourceCollection_Body);
			AddNew(CommandDefs._9_UsingDocDataValueOnADataSource_Title
				 , CommandDefs._9_UsingDocDataValueOnADataSource_Body);
		}

		SyntaxAndFormattingWrapper AddNew(ZString titleText, ZString lineText)
		{
			var chapter = new SyntaxAndFormattingWrapper(titleText, lineText, Factory);
			Add(chapter);
			return chapter;
		}

		class CommandDefs
		{
			#region _1_UsingFormatOnADataSource
			public const string _1_UsingFormatOnADataSource_Title = "Using Format() on a DataSource";
			public const string _1_UsingFormatOnADataSource_Body = @"Useage: [datasource.]Format(formatstring)

Let's say we have a related DocumentWrapper called ""Person"" with the 3 fields ""Name"", Suburb"", and ""Region"" where:

Person.Name = ""Fred""
Person.Suburb = ""Homebush""
Person.Region = ""North""

If we inserted the following syntax in a cell:

<Person.Format(""{Name} from {Suburb} – Region: {Region}"")>

The resulting text shown when the document was rendered would be:

Fred from Homebush – Region: North

Any block of text wrapped in curly braces ( ""{"" and ""}"" ) within a Format String is replaced with the contents of the field specified by the block of text within the curly braces as long as a valid field name is specified. 

The curly braces are also removed, and any text not enclosed within curly braces is simply included as part of the result so you can specify your own spacing, delimiters or separators as required.
";
			#endregion

			#region _2_FormatCodes
			public const string _2_FormatCodes_Title = "Format Codes in Format Strings";
			public const string _2_FormatCodes_Body = @"When inserting fields in a formatstring, you can optionally specify a formatcode for each fieldname wrapped in curly braces within the formatstring. You can use a formatcode to determine how fields are presented within the document.

Instead of just inserting {fieldname} you can use {fieldname:formatcode} which will present the same data, but in the format designated by whatever format code you specify.";
			#endregion

			#region _3_StringFormatCodes
			public const string _3_StringFormatCodes_Title = "String Field Format Codes";
			public const string _3_StringFormatCodes_Body = @"{StringField:Upper} – Contents of the field converted to UPPER case.
{StringField:Lower} – Contents of the field converted to lower case.
{StringField:Proper} – Contents of the field converted to Proper case.";
			#endregion

			#region _4_DateFormatCodes
			public const string _4_DateFormatCodes_Title = "Date Field Format Codes";
			public const string _4_DateFormatCodes_Body = @"{DateField:Date} – Standard Date Format – eg: 23-Jan-06
{DateField:LongDate} – Long Date Format – eg: Monday, January 23rd, 2006
{DateField:AmericanDate} – American Date Format – eg: 01/23/06
{DateField:AmericanDateWithCentury} – American Date Format – eg: 01/23/2006
{DateField:BritishDate} – British Date Format – eg: 23/01/06
{DateField:Day} – Day of the Week – eg: Monday
{DateField:Time24h} – 24 hour Format Time – eg: 18:34
{DateField:Time12h} – 12 hour Format Time – eg: 6:34p
{DateField:SpanishDate} – Spanish Date Format – eg: 23-01-2006";
			#endregion

			#region _5_BooleanFormatCodes
			public const string _5_BooleanFormatCodes_Title = "Boolean Field Format Codes";
			public const string _5_BooleanFormatCodes_Body = @"{BooleanField:X} - Returns either X for true or a blank string. Used for making CheckBoxes work.
{BooleanField:YN} - Returns either Y or N. 
{BooleanField:YesNo} - Returns either Yes or No. 
{BooleanField:TrueFalse} - Returns either True or False.";
			#endregion

			#region _6_NumericFormatCodes
			public const string _6_NumericFormatCodes_Title = "Numeric Field Format Codes";
			public const string _6_NumericFormatCodes_Body = @"Numeric Formatters consist of a Single Letter followed by an optional 1 or 2 digit number known as the Precision Indicator. The letter indicates the basic format used to format the number, and the Precision Indicator gives addition information on how the number should be formatted.
 
Numeric Format Letters:
 
C - Currency - Converted to a string that represents a currency amount. The conversion is controlled by the currency format information of the NumberFormatInfo object used to format the number. The precision specifier indicates the desired number of decimal places. If the precision specifier is omitted, the default currency precision given by the NumberFormatInfo is used.
eg: {NumericField:C2} - $1.99
 
D – Decimal - Converted to a string of decimal digits (0-9), prefixed by a minus sign if the number is negative. The precision specifier indicates the minimum number of digits desired in the resulting string. If required, the number is padded with zeros to its left to produce the number of digits given by the precision specifier.
eg: {NumericField:D10} - 0000001.99
 
E - Scientific (exponential) - Converted to a string of the form ""-d.ddd…E+ddd"", where each 'd' indicates a digit (0-9). The string starts with a minus sign if the number is negative. One digit always precedes the decimal point. The precision specifier indicates the desired number of digits after the decimal point. If the precision specifier is omitted, a default of six digits after the decimal point is used. The exponent always consists of a plus or minus sign and a minimum of three digits. The exponent is padded with zeros to meet this minimum, if required.
eg: {NumericField:E6} - 1.990000E+0
 
F - Fixed-point - Converted to a string of the form ""-ddd.ddd…"" where each 'd' indicates a digit (0-9). The string starts with a minus sign if the number is negative. The precision specifier indicates the desired number of decimal places. If the precision specifier is omitted, the default numeric precision given by the NumberFormatInfo is used.
eg: {NumericField:F3} - 1.990
 
N – Number - Converted to a string of the form ""-d,ddd,ddd.ddd…"", where each 'd' indicates a digit (0-9). The string starts with a minus sign if the number is negative. Thousand separators are inserted between each group of three digits to the left of the decimal point. The precision specifier indicates the desired number of decimal places. If the precision specifier is omitted, the default numeric precision given by the NumberFormatInfo is used.
eg: {NumericField:N4} - 4,343,122.0040
 
P – Percentage - Converted to a string of the form ""-d,ddd.ddd…"", where each 'd' indicates a digit (0-9). Value provided is multiplied by 100 to make a percentage, and starts with a minus sign if the number is negative. A percentage symbol is added as a suffix to the expression returned with a space between the % and the last digit. Thousand separators are inserted between each group of three digits to the left of the decimal point. The precision specifier indicates the desired number of decimal places. If the precision specifier is omitted, the default numeric precision given by the NumberFormatInfo is used.
eg: {NumericField:P1} - 434,312,200.4 %
 
T – Trim Trailing Zeros - Converted to a string of the form ""-dddd.ddd…"", where each 'd' indicates a digit (0-9). The string starts with a minus sign if the number is negative. The precision specifier indicates the maximum desired number of decimal places. All trailing zeros are removed after the value has been rounded to the specified number of decimal places. If the precision specifier is omitted, the default numeric precision given by the NumberFormatInfo is used but all trailing zeros are removed.
eg: {NumericField:T4} - 4343122.004";
			#endregion

			#region _7_UsingFormatOnADataSourceCollection
			public const string _7_UsingFormatOnADataSourceCollection_Title = "DataSourceCollection.Format()";
			public const string _7_UsingFormatOnADataSourceCollection_Body = @"Useage: datasourcecollection.Format(formatstring [, delimiter [, filter [, groupbystring [, maxItems]]]] )

You can also use the Format() function on any DataSource Collection. Format() will evaluate the formatstring on each DataSource in the specified Collection and return a concatenation of the evaluated expressions delimited by the specified delimiter. If you don't specify a delimiter, Comma will be used as the default.

You can optionally specify one of the following delimiters used to separate the data from each DataSource in the Collection:
			Comma
			Space
			NewLine
			Dash
			Colon
If you don't specify a delimiter, Comma will be used by default.

Let's say you had a Collection of Container DataSources called Containers. There are 2 containers in the Collection with a ContainerNumber and Mode.

<Containers.Format(""{ContainerNumber} ({Mode})"", Comma)>
would be replaced with:
OOCL0000006 (FCL), OOCL0000011 (LCL)

<Containers.Format(""{ContainerNumber} ({Mode})"", NewLine)>
would be replaced with:
OOCL0000006 (FCL)<cr><lf>
OOCL0000011 (LCL)

NB: Please note that for the 'NewLine' delimiter to work across multiple lines, you need to either format the cell to have 'Wrap text' turned on (right click on the cell, select 'Format Cells', go to 'Alignment' tab), or use the <AutoHeight> macro like so:-

<AutoHeight><Containers.Format(""{ContainerNumber} ({Mode})"", NewLine)>

You can use a filter as the 3rd argument. The filter applies to properties on items from the datasource collection.
If you don't specify a filter, the empty filter """" will be used by default.

Let's say you had a Collection of Container DataSources called Containers. There are 3 containers in the Collection with a ContainerNumber and Mode.

<Containers.Format(""{ContainerNumber} ({Mode})"", Comma, """")>
would be replaced with:
OOCL0000006 (FCL), OOCL0000011 (LCL), OOCL0000015 (FCL)

<Containers.Format(""{ContainerNumber} ({Mode})"", Comma, ""{Mode}"" == ""FCL"")>
would be replaced with:
OOCL0000006 (FCL), OOCL0000015 (FCL)

You can also use a group-by string as the 4th argument. When specified, the elements will be grouped by the values specified in the group by string and the format string will be evaluated only once for each group. Also, the format string will evaluate {Count} as the number of elements in the group.

Let's say you had a Collection of Container DataSources called Containers. There are 5 containers in the Collection, 3 are 20GP containers and 2 are 40GP containers.

<Containers.Format(""{Count} x {Type})"", Comma, """", ""{Type}"")>
would be replaced with:
3 x 20GP - Twenty Foot General Purpose, 2 x 40GP - Fourty Foot General Purpose


You can specify the maximum number of items to be formatted.
If you also specify a Group-by string, the maximun will be applied to the grouped strings.

Let's say you had a Collection of Container DataSources called Containers. There are 3 containers in the Collection with a ContainerNumber.

<Containers.Format(""{ContainerNumber}"", Comma, """", """", 3)>
would be replaced with:
OOCL0000006, OOCL0000011, OOCL0000015

<Containers.Format(""{ContainerNumber}"", Comma, """", """", 2)>
would be replaced with:
OOCL0000006, OOCL0000011

If the maximun number is less than 1, the parameter will be ignored.";

			#endregion

			#region _8_UsingTotalOnADataSourceCollection

			public const string _8_UsingTotalOnADataSourceCollection_Title = "DataSourceCollection.Total()";
			public const string _8_UsingTotalOnADataSourceCollection_Body = @"Useage: datasourcecollection.Total(fieldname [, decimalplaces, [filter]])

You can use the Total() function on any DataSource Collection. Total() will provide a string representation of the total value of the field specified from each DataSource in the Collection. 

If you don't specify a number of decimal places, zero is assumed, and if the result is zero, a blank string is returned.

The filter parameter is a conditional statement calculated for each item in the collection (eg. ""{GS_IsActive}"" == ""Y"" (is staff member active)). If the condition is false, the item is not included in the calculation of the Total.";
			#endregion

			#region _9_UsingDocDataValueOnADataSource
			public const string _9_UsingDocDataValueOnADataSource_Title = "DataSource.DocDataValue()";
			public const string _9_UsingDocDataValueOnADataSource_Body = @"Useage: [datasource.]DocDataValue(docdatafieldname [, fallbackformatstring]) ";
			#endregion

			//#region _1_XXXX
			//public const string _1_XXXX_Title = "";
			//public const string _1_XXXX_Body = @"";
			//#endregion
		}
	}
	#endregion
}
