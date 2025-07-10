READ ME
a) Obtain in tab-delimited format three sets of data and embed them into the relevant files.
	1. List of all possible suffixes from https://www.gov.uk/guidance/additional-procedure-codes-for-data-element-111-of-cds
	2. List of all possible prefixes from https://www.gov.uk/guidance/requested-and-previous-procedure-codes-for-de-110-of-cds
	3. Cross-reference of all prefixes and suffixes from https://www.gov.uk/guidance/4-digit-to-3-digit-procedure-to-additional-procedure-code-correlation-matrix

	In each case, just copy & paste the text from the webpage into the text file, then clean-up (remove chapter headings, etc)
	Put them into the Suffix.txt, Prefix.txt and CrossReference.txt files.

b) Then obtain the "Declaration Technical completion matrix" and select the "Proceduire Category Derivation" tab.
	1. At the time of writitng this is available in folder 03 of the HMRC Google Drive. https://drive.google.com/drive/u/0/folders/1MnP0iHeZwIL0MeNa-5V_I1AxLgoqKc_c
	2. Remove all but the first (procedure) and last (procedure category) columns in Excel.
	3. Swap these two columns areound, so Cateogry is the first column, using Excel. 
	4. Then grab the text and paste it into Category.txt

c) Manually find-and-replace ODG for 0GD in the CrossReference file. They made a typo. 

d) Run the unit test, it will fail if the data has changed. Replace the content of the expections with what you've now generated.  Attach the same to the WI and ask BP to fire that SQL. 

e) I have included the above web files in this project, for reference only.  If you need to obtain the latest procedure codes, you must download them afresh!


Update for inventory.  Rather annoyingly, the procedures for inventory are given to us in a different format.  A matrix with ticks, and not a table comprising multiple values. So we need another approach for inventory.
a) Use the embedded XLS file and Word doc to create the list of suffixes for inventory and put them into the file SuffixInventory.txt.  Due to HMRC re-using two codes (000 and 47C) we cannot put these new suffixes in the existing file, which was my preference.
b) Paste the content of the XLS sheets into the CorossReferneceInventoryImport/Export file. Keep the ticks, just get rid of new lines and plain text.A new procedure will turn the ticks into text.
c) Put the new inventory procedures into the Prefix file - take them from the XLS sheets' first column. Make the description sensible/friendly.
d) We have no information about which procedures can be used with whcih declaration categories.  Assume (reasonably) that they will only be used with C21 declarations.  Put 21I and 21E against the 4-char procedure code in the Category.txt file. 
e) Clean each file for dodgy line-ending chars (want only \r\n, not \n), fancy quote marks, etc. 
f) Manually add suffix records for 31E, 31O, 78R - these are used in the export invnetoyr cross-reference, but not defined in the word doc, so invent our own explanations. 
