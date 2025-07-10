

CIMXXX
This is a simple wrapper for CargoImp messages.  The result is a simple edifact message with some FTX segments that will hold the CargoImp message.

For example, hewre is the CargoImp FCS message:
	FCS
	LHRTWA
	015-12345675
	SPT/01P2K33
	SPT/02P2K80
	SPT/03P1K1

Here is the shell for the CIMXXX message:
	UNH+MSGREF+CIMXXX:0:0:IA+01512345675'
	FTX+CIM+++cargoImp Here
	FTX+CIM+++continued'
	FTX+CIM+++continued'
	UNT+5+MSGREF'

All together (expanded for clarity):
	UNH+MSGREF+CIMFCS:0:0:IA+01512345675'
	FTX+CIM+++
		FCS:
		LHRTWA:
		015-12345675:
		SPT/01P2K33:
		SPT/02P2K80'
	FTX+CIM+++
		SPT/03P1K1'
	UNT+4+MSGREF'



I have no informationabout the number of FTXes allowed so I am defining 999.