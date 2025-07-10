lexer grammar LegacyMacroLexer;

// ----------------- Everything OUTSIDE of a STRING ---------------------

NULL
    : 'null'
    ;

TRUE
    : 'true'
    ;

FALSE
    : 'false'
    ;

NOT
    : '!'
    ;

SINGLEQUOTE
    : '\'' -> pushMode(SINGLEQUOTED_STRING)
    ;

DOUBLEQUOTE
    : '"' -> pushMode(DOUBLEQUOTED_STRING)
    ;

LOGICALAND
    : '&&'
    ;

LOGICALOR
    : '||'
    ;

LOGICALEQUALS
    : '=='
    ;

STRICTLOGICALEQUALS
    : '==='
    ;

LOGICALNOTEQUALS
    : '!='
    ;

STRICTLOGICALNOTEQUALS
    : '!=='
    ;

GREATERTHAN
    : '>'
    ;

HTMLGREATERTHAN
    : '&gt;'
    ;

GREATERTHANOREQUALS
    : '>='
    ;

HTMLGREATERTHANOREQUALS
    : '&gt;='
    ;

LESSTHAN
    : '<'
    ;

HTMLLESSTHAN
    : '&lt;'
    ;

LESSTHANOREQUALS
    : '<='
    ;

HTMLLESSTHANOREQUALS
    : '&lt;='
    ;

MULTIPLY
    : '*'
    ;

DIVIDE
    : '/'
    ;

PLUS
    : '+'
    ;

MINUS
    : '-'
    ;

FULLSTOP
    : '.'
    ;

COMMA
    : ','
    ;

LPAREN
    : '('
    ;

RPAREN
    : ')'
    ;

SUBSTR
    : 'substr'
    | 'Substring'
    ;

INDEXOF
    : 'indexOf'
    | 'IndexOf'
    ;

CONTAINS
    : 'Contains'
    ;

TOLOWER
    : 'toLowerCase'
    | 'ToLower'
    ;

STARTSWITH
    : 'startsWith'
    | 'StartsWith'
    ;

ENDSWITH
    : 'endsWith'
    | 'EndsWith'
    ;

fragment Digit
    : ('0'..'9')
    ;

fragment Letter
    : ('a'..'z'|'A'..'Z')
    ;

fragment NewLine
    : '\r'? '\n'
    ;

fragment Space
    : (' '|'\t'|'\u000C')+
    ;

MEMBERNAME
    : (Letter|'_') (Letter|Digit|'_')*
    ;

NUMBER
    : Digit+ ('.' Digit+)?
    ;

WS
    : (NewLine | Space) -> channel(HIDDEN)
    ;

// ----------------- Everything INSIDE of a DOUBLE QUOTED STRING ---------------------

mode DOUBLEQUOTED_STRING;

DQ_DOUBLEQUOTE
	: '"' -> type(DOUBLEQUOTE), popMode
	;

DQ_ESCAPEDESCAPE
	:'\\\\'
	;

DQ_ESCAPEDDOUBLEQUOTE
	:'\\"'
	;

DQ_ANY
	: .
	;

// ----------------- Everything INSIDE of a SINGLE QUOTED STRING ---------------------

mode SINGLEQUOTED_STRING;

SQ_DOUBLEQUOTE
	: '\'' -> type(SINGLEQUOTE), popMode
	;

SQ_ESCAPEDESCAPE
	:'\\\\'
	;

SQ_ESCAPEDSINGLEQUOTE
	:'\\\''
	;

SQ_ANY
	: .
	;